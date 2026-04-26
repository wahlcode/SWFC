namespace SWFC.Infrastructure.M400_Integration.M405_MessagingEvents;

public sealed record IntegrationEventEnvelope(
    string EventName,
    string SourceModule,
    string CorrelationId,
    DateTime OccurredAtUtc,
    IReadOnlyDictionary<string, object?> Payload);

public sealed record IntegrationEventDelivery(
    string EventName,
    string HandlerName,
    bool Delivered,
    string? Message);

public interface IIntegrationEventHandler
{
    string EventName { get; }

    string HandlerName { get; }

    Task<IntegrationEventDelivery> HandleAsync(
        IntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}

public interface IIntegrationMessageTransport
{
    Task PublishAsync(
        IntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}

public sealed class IntegrationEventDispatcher
{
    private readonly IEnumerable<IIntegrationEventHandler> _handlers;
    private readonly IIntegrationMessageTransport? _transport;

    public IntegrationEventDispatcher(
        IEnumerable<IIntegrationEventHandler> handlers,
        IIntegrationMessageTransport? transport = null)
    {
        _handlers = handlers;
        _transport = transport;
    }

    public async Task<IReadOnlyList<IntegrationEventDelivery>> PublishAsync(
        IntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        Validate(envelope);

        if (_transport is not null)
        {
            await _transport.PublishAsync(envelope, cancellationToken);
        }

        var deliveries = new List<IntegrationEventDelivery>();
        var matchingHandlers = _handlers.Where(handler =>
            string.Equals(handler.EventName, envelope.EventName, StringComparison.OrdinalIgnoreCase));

        foreach (var handler in matchingHandlers)
        {
            deliveries.Add(await handler.HandleAsync(envelope, cancellationToken));
        }

        return deliveries;
    }

    private static void Validate(IntegrationEventEnvelope envelope)
    {
        if (string.IsNullOrWhiteSpace(envelope.EventName))
            throw new InvalidOperationException("Event name is required.");

        if (string.IsNullOrWhiteSpace(envelope.SourceModule))
            throw new InvalidOperationException("Source module is required.");

        if (string.IsNullOrWhiteSpace(envelope.CorrelationId))
            throw new InvalidOperationException("Correlation id is required.");

        if (envelope.OccurredAtUtc == default)
            throw new InvalidOperationException("Event timestamp is required.");
    }
}
