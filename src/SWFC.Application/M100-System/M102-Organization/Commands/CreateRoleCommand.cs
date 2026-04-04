namespace SWFC.Application.M100_System.M102_Organization.Commands;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    string Reason);