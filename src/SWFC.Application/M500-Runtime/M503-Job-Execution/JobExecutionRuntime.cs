namespace SWFC.Application.M500_Runtime.M503_Job_Execution;

public enum RuntimeJobStatus
{
    Queued = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    RetryScheduled = 4,
    Stopped = 5
}

public sealed record RuntimeJobRequest(
    string JobKey,
    string SourceModule,
    string CorrelationId,
    DateTime RequestedAtUtc,
    IReadOnlyDictionary<string, string> Parameters,
    int MaxRetries = 0);

public sealed record RuntimeJobHandlerResult(bool Succeeded, string Message)
{
    public static RuntimeJobHandlerResult Success(string message = "Job completed.") => new(true, message);

    public static RuntimeJobHandlerResult Failure(string message) => new(false, message);
}

public sealed record RuntimeJobRun(
    Guid RunId,
    RuntimeJobRequest Request,
    RuntimeJobStatus Status,
    int Attempt,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    string Message);

public interface IRuntimeJobHandler
{
    string JobKey { get; }

    Task<RuntimeJobHandlerResult> HandleAsync(
        RuntimeJobRequest request,
        CancellationToken cancellationToken = default);
}

public interface IRuntimeJobExecutor
{
    Task<RuntimeJobRun> StartAsync(
        RuntimeJobRequest request,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<RuntimeJobRun> RetryAsync(
        Guid runId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    RuntimeJobRun Stop(Guid runId, DateTime utcNow, string reason);

    IReadOnlyList<RuntimeJobRun> GetRuns();
}

public sealed class RuntimeJobExecutor : IRuntimeJobExecutor
{
    private readonly IReadOnlyDictionary<string, IRuntimeJobHandler> _handlers;
    private readonly List<RuntimeJobRun> _runs = [];

    public RuntimeJobExecutor(IEnumerable<IRuntimeJobHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        _handlers = handlers
            .GroupBy(handler => handler.JobKey, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    public Task<RuntimeJobRun> StartAsync(
        RuntimeJobRequest request,
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(request, utcNow, attempt: 1, cancellationToken);

    public async Task<RuntimeJobRun> RetryAsync(
        Guid runId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var previous = _runs.SingleOrDefault(x => x.RunId == runId)
            ?? throw new InvalidOperationException($"Runtime job run '{runId}' is not known.");

        if (previous.Status != RuntimeJobStatus.Failed && previous.Status != RuntimeJobStatus.RetryScheduled)
        {
            throw new InvalidOperationException("Only failed or retry-scheduled jobs can be retried.");
        }

        if (previous.Attempt > previous.Request.MaxRetries)
        {
            throw new InvalidOperationException("Maximum retry count has already been reached.");
        }

        return await ExecuteAsync(previous.Request, utcNow, previous.Attempt + 1, cancellationToken);
    }

    public RuntimeJobRun Stop(Guid runId, DateTime utcNow, string reason)
    {
        var index = _runs.FindIndex(x => x.RunId == runId);

        if (index < 0)
        {
            throw new InvalidOperationException($"Runtime job run '{runId}' is not known.");
        }

        var run = _runs[index];

        if (run.Status is RuntimeJobStatus.Succeeded or RuntimeJobStatus.Failed or RuntimeJobStatus.Stopped)
        {
            throw new InvalidOperationException("Completed runtime job runs cannot be stopped.");
        }

        var stopped = run with
        {
            Status = RuntimeJobStatus.Stopped,
            CompletedAtUtc = utcNow,
            Message = string.IsNullOrWhiteSpace(reason) ? "Job stopped." : reason.Trim()
        };

        _runs[index] = stopped;
        return stopped;
    }

    public IReadOnlyList<RuntimeJobRun> GetRuns() =>
        _runs.OrderBy(x => x.StartedAtUtc).ThenBy(x => x.RunId).ToArray();

    private async Task<RuntimeJobRun> ExecuteAsync(
        RuntimeJobRequest request,
        DateTime utcNow,
        int attempt,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);
        request = Normalize(request);

        var running = new RuntimeJobRun(
            Guid.NewGuid(),
            request,
            RuntimeJobStatus.Running,
            attempt,
            utcNow,
            null,
            "Job started.");

        _runs.Add(running);

        if (!_handlers.TryGetValue(request.JobKey, out var handler))
        {
            return Complete(running, RuntimeJobStatus.Failed, utcNow, $"No runtime job handler registered for '{request.JobKey}'.");
        }

        try
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            var status = result.Succeeded
                ? RuntimeJobStatus.Succeeded
                : attempt <= request.MaxRetries
                    ? RuntimeJobStatus.RetryScheduled
                    : RuntimeJobStatus.Failed;

            return Complete(running, status, utcNow, result.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var status = attempt <= request.MaxRetries
                ? RuntimeJobStatus.RetryScheduled
                : RuntimeJobStatus.Failed;

            return Complete(running, status, utcNow, ex.Message);
        }
    }

    private RuntimeJobRun Complete(
        RuntimeJobRun running,
        RuntimeJobStatus status,
        DateTime utcNow,
        string message)
    {
        var completed = running with
        {
            Status = status,
            CompletedAtUtc = utcNow,
            Message = string.IsNullOrWhiteSpace(message) ? status.ToString() : message.Trim()
        };

        var index = _runs.FindIndex(x => x.RunId == running.RunId);
        _runs[index] = completed;
        return completed;
    }

    private static void Validate(RuntimeJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.JobKey) ||
            string.IsNullOrWhiteSpace(request.SourceModule) ||
            string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            throw new ArgumentException("Job key, source module and correlation id are required.");
        }

        if (request.MaxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Retry count cannot be negative.");
        }
    }

    private static RuntimeJobRequest Normalize(RuntimeJobRequest request) =>
        request with
        {
            JobKey = request.JobKey.Trim(),
            SourceModule = request.SourceModule.Trim(),
            CorrelationId = request.CorrelationId.Trim(),
            Parameters = ToSortedDictionary(request.Parameters)
        };

    private static SortedDictionary<string, string> ToSortedDictionary(
        IReadOnlyDictionary<string, string>? values) =>
        values is null
            ? new SortedDictionary<string, string>(StringComparer.Ordinal)
            : new SortedDictionary<string, string>(
                values.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal),
                StringComparer.Ordinal);
}
