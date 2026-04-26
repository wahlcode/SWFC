using System.Collections.Concurrent;

namespace SWFC.Infrastructure.M400_Integration.M404_IoTMaschinen;

public enum MachineTelemetryKind
{
    State,
    Measurement,
    Event
}

public sealed record MachineTelemetryPoint(
    string Name,
    MachineTelemetryKind Kind,
    string RawValue,
    string? Unit,
    string? TargetModule);

public sealed record MachineTelemetryPacket(
    string SourceSystem,
    string ExternalMachineKey,
    DateTime OccurredAtUtc,
    IReadOnlyList<MachineTelemetryPoint> Points,
    IReadOnlyDictionary<string, string?> Metadata);

public sealed record MachineTelemetryEnvelope(
    string SourceSystem,
    string ExternalMachineKey,
    DateTime OccurredAtUtc,
    IReadOnlyList<MachineTelemetryPoint> Points,
    IReadOnlyDictionary<string, string?> Metadata,
    IReadOnlyList<string> SupportedConsumerModules);

public interface IMachineTelemetrySink
{
    Task ForwardAsync(
        MachineTelemetryEnvelope envelope,
        CancellationToken cancellationToken = default);
}

public sealed class InProcessMachineTelemetrySink : IMachineTelemetrySink
{
    private readonly ConcurrentQueue<MachineTelemetryEnvelope> _envelopes = new();

    public IReadOnlyCollection<MachineTelemetryEnvelope> Envelopes => _envelopes.ToArray();

    public Task ForwardAsync(
        MachineTelemetryEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        _envelopes.Enqueue(envelope);
        return Task.CompletedTask;
    }
}

public sealed class MachineTelemetryAdapter
{
    private static readonly IReadOnlyList<string> ConsumerModules = ["M205", "M212", "M303"];
    private readonly IMachineTelemetrySink _sink;

    public MachineTelemetryAdapter(IMachineTelemetrySink sink)
    {
        _sink = sink;
    }

    public async Task<MachineTelemetryEnvelope> TransferAsync(
        MachineTelemetryPacket packet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Validate(packet);

        var envelope = new MachineTelemetryEnvelope(
            packet.SourceSystem.Trim(),
            packet.ExternalMachineKey.Trim(),
            packet.OccurredAtUtc,
            packet.Points
                .Select(point => point with
                {
                    Name = point.Name.Trim(),
                    RawValue = point.RawValue.Trim(),
                    Unit = string.IsNullOrWhiteSpace(point.Unit) ? null : point.Unit.Trim(),
                    TargetModule = string.IsNullOrWhiteSpace(point.TargetModule) ? null : point.TargetModule.Trim()
                })
                .ToArray(),
            packet.Metadata,
            ConsumerModules);

        await _sink.ForwardAsync(envelope, cancellationToken);
        return envelope;
    }

    private static void Validate(MachineTelemetryPacket packet)
    {
        if (string.IsNullOrWhiteSpace(packet.SourceSystem))
            throw new InvalidOperationException("Telemetry source system is required.");

        if (string.IsNullOrWhiteSpace(packet.ExternalMachineKey))
            throw new InvalidOperationException("External machine key is required.");

        if (packet.OccurredAtUtc == default)
            throw new InvalidOperationException("Telemetry timestamp is required.");

        if (packet.Points.Count == 0)
            throw new InvalidOperationException("At least one telemetry point is required.");

        foreach (var point in packet.Points)
        {
            if (string.IsNullOrWhiteSpace(point.Name))
                throw new InvalidOperationException("Telemetry point name is required.");

            if (string.IsNullOrWhiteSpace(point.RawValue))
                throw new InvalidOperationException($"Telemetry point '{point.Name}' has no raw value.");
        }
    }
}
