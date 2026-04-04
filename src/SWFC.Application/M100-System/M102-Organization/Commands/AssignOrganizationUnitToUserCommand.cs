namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record AssignOrganizationUnitToUserCommand(
    Guid UserId,
    Guid OrganizationUnitId,
    bool IsPrimary,
    string Reason);