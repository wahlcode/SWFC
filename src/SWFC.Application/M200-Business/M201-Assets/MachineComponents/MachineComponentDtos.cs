namespace SWFC.Application.M200_Business.M201_Assets.MachineComponents;

public sealed record MachineComponentListItemDto(
    Guid Id,
    Guid MachineId,
    Guid? MachineComponentAreaId,
    Guid? ParentMachineComponentId,
    string Name,
    string Description,
    bool IsActive,
    int Level,
    bool HasChildren,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
