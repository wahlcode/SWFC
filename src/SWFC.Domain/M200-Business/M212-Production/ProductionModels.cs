namespace SWFC.Domain.M200_Business.M212_Production;

public enum MachineRuntimeState
{
    Running,
    Stopped,
    Fault,
    Setup
}

public enum ProductionOrderStatus
{
    Planned,
    Running,
    Interrupted,
    Completed,
    Archived
}

public sealed record ProductionFeedback(
    Guid MachineId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset EndedAtUtc,
    MachineRuntimeState RuntimeState,
    decimal ProducedQuantity,
    decimal ScrapQuantity,
    string Unit,
    string? DowntimeReason)
{
    public decimal GoodQuantity => ProducedQuantity - ScrapQuantity;
    public bool CanTriggerQualityCase => ScrapQuantity > 0;
    public bool CanCorrelateEnergy => MachineId != Guid.Empty && ProducedQuantity > 0;
}

public sealed class ProductionOrder
{
    private readonly List<ProductionFeedback> _feedback = new();

    public ProductionOrder(Guid id, string orderNumber, Guid machineId, string product, decimal plannedQuantity, string unit)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        OrderNumber = RequireText(orderNumber, nameof(orderNumber));
        MachineId = machineId == Guid.Empty ? throw new ArgumentException("Machine is required.", nameof(machineId)) : machineId;
        Product = RequireText(product, nameof(product));
        PlannedQuantity = plannedQuantity > 0 ? plannedQuantity : throw new ArgumentOutOfRangeException(nameof(plannedQuantity));
        Unit = RequireText(unit, nameof(unit));
        Status = ProductionOrderStatus.Planned;
    }

    public Guid Id { get; }
    public string OrderNumber { get; }
    public Guid MachineId { get; }
    public string Product { get; }
    public decimal PlannedQuantity { get; }
    public string Unit { get; }
    public ProductionOrderStatus Status { get; private set; }
    public IReadOnlyList<ProductionFeedback> Feedback => _feedback;
    public decimal ProducedQuantity => _feedback.Sum(x => x.ProducedQuantity);
    public decimal ScrapQuantity => _feedback.Sum(x => x.ScrapQuantity);

    public void Start()
    {
        Status = ProductionOrderStatus.Running;
    }

    public void AddFeedback(ProductionFeedback feedback)
    {
        if (feedback.MachineId != MachineId)
        {
            throw new ArgumentException("Feedback must belong to the production order machine.", nameof(feedback));
        }

        _feedback.Add(feedback);
        Status = feedback.RuntimeState == MachineRuntimeState.Fault
            ? ProductionOrderStatus.Interrupted
            : ProductionOrderStatus.Running;
    }

    public void Complete()
    {
        Status = ProductionOrderStatus.Completed;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
