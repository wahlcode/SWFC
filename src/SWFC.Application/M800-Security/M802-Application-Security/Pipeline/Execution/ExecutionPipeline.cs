using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;

public sealed class ExecutionPipeline<TRequest, TResponse>
    : IExecutionPipeline<TRequest, TResponse>
{
    private readonly IEnumerable<IPipelineStep<TRequest, TResponse>> _steps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUseCaseHandler<TRequest, TResponse> _handler;

    public ExecutionPipeline(
        IEnumerable<IPipelineStep<TRequest, TResponse>> steps,
        ICurrentUserService currentUserService,
        IUseCaseHandler<TRequest, TResponse> handler)
    {
        _steps = steps;
        _currentUserService = currentUserService;
        _handler = handler;
    }

    public async Task<Result<TResponse>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var context = new PipelineContext<TRequest>(request, securityContext);

        foreach (var step in _steps)
        {
            var result = await step.ExecuteAsync(context, cancellationToken);

            if (!result.IsSuccess)
            {
                return Result<TResponse>.Failure(result.Error);
            }
        }

        return await _handler.HandleAsync(request, cancellationToken);
    }
}

