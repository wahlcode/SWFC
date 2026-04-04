namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record RemoveOrganizationUnitFromUserCommand(
    Guid UserId,
    Guid OrganizationUnitId,
    string Reason);