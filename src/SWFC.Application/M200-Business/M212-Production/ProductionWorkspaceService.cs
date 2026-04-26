using SWFC.Domain.M200_Business.M212_Production;

namespace SWFC.Application.M200_Business.M212_Production;

public sealed record ProductionOrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string Product,
    string Status,
    decimal PlannedQuantity,
    decimal ProducedQuantity,
    decimal ScrapQuantity,
    string Unit,
    bool HasQualitySignal,
    bool HasEnergyCorrelation);

public sealed class ProductionWorkspaceService
{
    private readonly List<ProductionOrder> _orders = new();

    public ProductionWorkspaceService()
    {
        var order = new ProductionOrder(
            Guid.Parse("21200000-0000-0000-0000-000000000001"),
            "PO-2026-0426",
            Guid.Parse("20100000-0000-0000-0000-000000000001"),
            "Baugruppe A",
            500,
            "pcs");
        order.Start();
        order.AddFeedback(new ProductionFeedback(
            order.MachineId,
            new DateTimeOffset(2026, 4, 26, 6, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 26, 10, 0, 0, TimeSpan.Zero),
            MachineRuntimeState.Running,
            220,
            4,
            "pcs",
            null));
        _orders.Add(order);
    }

    public IReadOnlyList<ProductionOrderSummaryDto> GetOrders()
    {
        return _orders
            .Select(order => new ProductionOrderSummaryDto(
                order.Id,
                order.OrderNumber,
                order.Product,
                order.Status.ToString(),
                order.PlannedQuantity,
                order.ProducedQuantity,
                order.ScrapQuantity,
                order.Unit,
                order.Feedback.Any(x => x.CanTriggerQualityCase),
                order.Feedback.Any(x => x.CanCorrelateEnergy)))
            .ToList();
    }
}
