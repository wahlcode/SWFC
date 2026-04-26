using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed record MaintenanceOrderMaterialDto(
    Guid Id,
    Guid ItemId,
    int Quantity,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record MaintenanceOrderListItemDto(
    Guid Id,
    string Number,
    string Title,
    MaintenanceOrderType Type,
    MaintenanceOrderStatus Status,
    MaintenanceOrderPriority Priority,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    Guid? MaintenancePlanId,
    DateTime? PlannedStartUtc,
    DateTime? PlannedEndUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record MaintenanceOrderDetailsDto(
    Guid Id,
    string Number,
    string Title,
    string Description,
    MaintenanceOrderType Type,
    MaintenanceOrderStatus Status,
    MaintenanceOrderPriority Priority,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    Guid? MaintenancePlanId,
    DateTime? PlannedStartUtc,
    DateTime? PlannedEndUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? DueAtUtc,
    IReadOnlyList<MaintenanceOrderMaterialDto> Materials,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
