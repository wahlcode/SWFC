using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M800_Security.M806_AccessControl.Users;

public sealed record SecurityUserListItemDto(
    Guid Id,
    string Username,
    string DisplayName,
    string BusinessEmail,
    UserStatus Status,
    UserType UserType,
    IReadOnlyList<string> Roles,
    bool IsProtectedSuperAdmin,
    LocalCredentialStatusDto? CredentialStatus);

public sealed record SecurityUserDetailsDto(
    Guid Id,
    string IdentityKey,
    string Username,
    string DisplayName,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    string BusinessEmail,
    UserStatus Status,
    UserType UserType,
    IReadOnlyList<string> Roles,
    bool IsProtectedSuperAdmin,
    LocalCredentialStatusDto? CredentialStatus);
