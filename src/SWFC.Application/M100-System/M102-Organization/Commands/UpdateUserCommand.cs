namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Username,
    string DisplayName,
    bool IsActive,
    string Reason);