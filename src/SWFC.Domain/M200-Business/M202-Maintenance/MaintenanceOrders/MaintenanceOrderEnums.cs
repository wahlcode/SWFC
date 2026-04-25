namespace SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

public enum MaintenanceTargetType
{
    Machine = 1,
    MachineComponent = 2
}

public enum MaintenanceOrderType
{
    Planned = 1,
    Unplanned = 2,
    Emergency = 3
}

public enum MaintenanceOrderStatus
{
    Open = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
