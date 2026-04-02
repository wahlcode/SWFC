namespace SWFC.Application.M200_Business.M201_Assets.Queries;

public sealed record GetMachinesQuery;

public sealed record MachineListItem(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);