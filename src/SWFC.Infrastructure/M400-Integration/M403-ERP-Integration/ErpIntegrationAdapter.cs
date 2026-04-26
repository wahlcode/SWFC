using System.Collections.Concurrent;

namespace SWFC.Infrastructure.M400_Integration.M403_ERPIntegration;

public enum ErpObjectKind
{
    Supplier,
    Customer,
    PurchaseOrder
}

public enum ErpTransferDirection
{
    InboundFromErp,
    OutboundToErp
}

public sealed record ErpObjectReference(
    ErpObjectKind ObjectKind,
    string ErpSystem,
    string ErpObjectId,
    string DisplayName,
    IReadOnlyDictionary<string, string?> Attributes);

public sealed record ErpPurchaseOrderTransfer(
    Guid PurchaseOrderId,
    string OrderNumber,
    Guid SupplierId,
    string? ExistingErpReference,
    string? DocumentReference);

public sealed record ErpTransferMessage(
    ErpTransferDirection Direction,
    ErpObjectKind ObjectKind,
    string ErpSystem,
    string CorrelationId,
    IReadOnlyDictionary<string, object?> Payload);

public interface IErpTransportAdapter
{
    Task SubmitAsync(
        ErpTransferMessage message,
        CancellationToken cancellationToken = default);
}

public sealed class InProcessErpTransportAdapter : IErpTransportAdapter
{
    private readonly ConcurrentQueue<ErpTransferMessage> _messages = new();

    public IReadOnlyCollection<ErpTransferMessage> Messages => _messages.ToArray();

    public Task SubmitAsync(
        ErpTransferMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}

public sealed class ErpIntegrationAdapter
{
    private readonly IErpTransportAdapter _transportAdapter;

    public ErpIntegrationAdapter(IErpTransportAdapter transportAdapter)
    {
        _transportAdapter = transportAdapter;
    }

    public ErpTransferMessage MapInboundMasterData(ErpObjectReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        EnsureReference(reference.ErpSystem, nameof(reference.ErpSystem));
        EnsureReference(reference.ErpObjectId, nameof(reference.ErpObjectId));

        return new ErpTransferMessage(
            ErpTransferDirection.InboundFromErp,
            reference.ObjectKind,
            reference.ErpSystem,
            $"{reference.ErpSystem}:{reference.ObjectKind}:{reference.ErpObjectId}",
            new Dictionary<string, object?>
            {
                ["ErpObjectId"] = reference.ErpObjectId,
                ["DisplayName"] = reference.DisplayName,
                ["CommercialAuthority"] = "ERP",
                ["OperationalAuthority"] = "SWFC",
                ["Attributes"] = reference.Attributes
            });
    }

    public async Task<ErpTransferMessage> TransferPurchaseOrderAsync(
        string erpSystem,
        ErpPurchaseOrderTransfer purchaseOrder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(purchaseOrder);
        EnsureReference(erpSystem, nameof(erpSystem));
        EnsureReference(purchaseOrder.OrderNumber, nameof(purchaseOrder.OrderNumber));

        var message = new ErpTransferMessage(
            ErpTransferDirection.OutboundToErp,
            ErpObjectKind.PurchaseOrder,
            erpSystem,
            purchaseOrder.PurchaseOrderId.ToString("N"),
            new Dictionary<string, object?>
            {
                ["PurchaseOrderId"] = purchaseOrder.PurchaseOrderId,
                ["OrderNumber"] = purchaseOrder.OrderNumber,
                ["SupplierId"] = purchaseOrder.SupplierId,
                ["ExistingErpReference"] = purchaseOrder.ExistingErpReference,
                ["DocumentReference"] = purchaseOrder.DocumentReference,
                ["SourceModule"] = "M206"
            });

        await _transportAdapter.SubmitAsync(message, cancellationToken);
        return message;
    }

    private static void EnsureReference(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required for ERP transfer.");
        }
    }
}
