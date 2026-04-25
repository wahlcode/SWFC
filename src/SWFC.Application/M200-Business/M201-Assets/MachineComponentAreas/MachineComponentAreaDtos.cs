namespace SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;

public sealed record MachineComponentAreaListItemDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
