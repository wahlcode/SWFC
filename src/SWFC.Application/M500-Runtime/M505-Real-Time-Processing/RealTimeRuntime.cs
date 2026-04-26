namespace SWFC.Application.M500_Runtime.M505_Real_Time_Processing;

public enum RealTimeProcessorState
{
    Running = 0,
    Stopped = 1
}

public sealed record RealTimePoint(
    string Name,
    string RawValue,
    string? Unit,
    string TargetModule);

public sealed record RealTimeEnvelope(
    string SourceSystem,
    string StreamKey,
    DateTime ObservedAtUtc,
    IReadOnlyList<RealTimePoint> Points,
    IReadOnlyDictionary<string, string> Tags);

public sealed record RealTimeStateSnapshot(
    string StreamKey,
    DateTime ObservedAtUtc,
    IReadOnlyDictionary<string, string> CurrentValues,
    IReadOnlyDictionary<string, int> EventCountsByModule,
    string Status);

public interface IRealTimeProcessor
{
    RealTimeProcessorState State { get; }

    void Start();

    void Stop();

    RealTimeStateSnapshot Process(RealTimeEnvelope envelope);

    IReadOnlyList<RealTimeStateSnapshot> GetHistory();
}

public sealed class RealTimeProcessor : IRealTimeProcessor
{
    private readonly Dictionary<string, RealTimeStateSnapshot> _states = new(StringComparer.Ordinal);
    private readonly List<RealTimeStateSnapshot> _history = [];

    public RealTimeProcessorState State { get; private set; } = RealTimeProcessorState.Running;

    public void Start() => State = RealTimeProcessorState.Running;

    public void Stop() => State = RealTimeProcessorState.Stopped;

    public RealTimeStateSnapshot Process(RealTimeEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (State == RealTimeProcessorState.Stopped)
        {
            var stopped = new RealTimeStateSnapshot(
                envelope.StreamKey,
                envelope.ObservedAtUtc,
                new SortedDictionary<string, string>(StringComparer.Ordinal),
                new SortedDictionary<string, int>(StringComparer.Ordinal),
                "Stopped");

            _history.Add(stopped);
            return stopped;
        }

        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);

        foreach (var point in envelope.Points.OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(point.Name) ||
                string.IsNullOrWhiteSpace(point.TargetModule))
            {
                throw new InvalidOperationException("Real-time point name and target module are required.");
            }

            var normalizedValue = string.IsNullOrWhiteSpace(point.Unit)
                ? point.RawValue
                : $"{point.RawValue} {point.Unit}";

            values[point.Name.Trim()] = normalizedValue.Trim();
            counts[point.TargetModule.Trim()] = counts.TryGetValue(point.TargetModule.Trim(), out var current)
                ? current + 1
                : 1;
        }

        var snapshot = new RealTimeStateSnapshot(
            envelope.StreamKey,
            envelope.ObservedAtUtc,
            values,
            counts,
            "Processed");

        _states[envelope.StreamKey] = snapshot;
        _history.Add(snapshot);
        return snapshot;
    }

    public IReadOnlyList<RealTimeStateSnapshot> GetHistory() => _history.ToArray();
}
