using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed record UserReferenceDto(
    Guid? Id,
    string? Name,
    string? Code);

public sealed record UserDetailsDto(
    Guid Id,
    string IdentityKey,
    string Username,
    string DisplayName,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    string BusinessEmail,
    string BusinessPhone,
    string Plant,
    string Location,
    string Team,
    Guid? CostCenterId,
    UserReferenceDto? CostCenter,
    Guid? ShiftModelId,
    UserReferenceDto? ShiftModel,
    string JobFunction,
    string PreferredCultureName,
    UserStatus Status,
    UserType UserType,
    bool IsActive,
    IReadOnlyCollection<OrganizationUnitReference> OrganizationUnits,
    IReadOnlyCollection<UserHistoryListItemDto> History);

public sealed record UserListItem(
    Guid Id,
    string IdentityKey,
    string Username,
    string DisplayName,
    string EmployeeNumber,
    string BusinessEmail,
    string PreferredCultureName,
    Guid? CostCenterId,
    string? CostCenterName,
    string? CostCenterCode,
    Guid? ShiftModelId,
    string? ShiftModelName,
    string? ShiftModelCode,
    UserStatus Status,
    UserType UserType,
    bool IsActive);

public sealed record UserHistoryListItemDto(
    Guid Id,
    UserHistoryChangeType ChangeType,
    string Summary,
    string Reason,
    DateTime ChangedAtUtc,
    string ChangedByUserId);