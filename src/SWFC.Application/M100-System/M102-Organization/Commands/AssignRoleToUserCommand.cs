namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId,
    string Reason);