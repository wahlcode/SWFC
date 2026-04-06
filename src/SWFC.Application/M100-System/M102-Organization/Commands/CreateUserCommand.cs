namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record CreateUserCommand(
    string IdentityKey,
    string Username,
    string DisplayName,
    bool IsActive,
    string Reason);