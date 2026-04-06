namespace SWFC.Application.M100_System.M102_Organization.DTOs;

public sealed record OrganizationUnitReference(
    Guid Id,
    string Name,
    string Code);