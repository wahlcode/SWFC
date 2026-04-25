namespace SWFC.Application.M100_System.M102_Organization.Users.Delegations;

public sealed record UserDelegationDetailsDto(
    Guid Id,
    Guid UserId,
    Guid DelegateUserId,
    string DelegationType,
    DateTime ValidFromUtc,
    DateTime? ValidToUtc,
    bool IsActive);

public sealed record UserDelegationListItem(
    Guid Id,
    Guid UserId,
    Guid DelegateUserId,
    string DelegationType,
    DateTime ValidFromUtc,
    DateTime? ValidToUtc,
    bool IsActive);