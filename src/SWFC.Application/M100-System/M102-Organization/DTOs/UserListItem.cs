namespace SWFC.Application.M100_System.M102_Organization.DTOs;

public sealed record UserListItem(
    Guid Id,
    string IdentityKey,
    string DisplayName,
    bool IsActive);