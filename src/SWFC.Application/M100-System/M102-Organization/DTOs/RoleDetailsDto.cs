namespace SWFC.Application.M100_System.M102_Organization.DTOs;

public sealed record RoleDetailsDto(
    Guid Id,
    string Name,
    string? Description);