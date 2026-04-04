namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record CreateOrganizationUnitCommand(
    string Name,
    string Code,
    Guid? ParentOrganizationUnitId,
    string Reason);