using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;

public sealed record MaintenancePlanListItemDto(
    Guid Id,
    string Name,
    string Description,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    int IntervalValue,
    MaintenancePlanIntervalUnit IntervalUnit,
    DateTime NextDueAtUtc,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedBy);

public sealed record MaintenancePlanDetailsDto(
    Guid Id,
    string Name,
    string Description,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    int IntervalValue,
    MaintenancePlanIntervalUnit IntervalUnit,
    DateTime NextDueAtUtc,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
