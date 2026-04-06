namespace SWFC.Application.M100_System.M103_Authentication.DTOs;

public sealed record AuthenticationResultDto(
    bool Succeeded,
    Guid? UserId,
    string IdentityKey,
    string Username,
    string DisplayName,
    bool IsLockedOut,
    DateTimeOffset? LockoutUntilUtc,
    string? FailureReason)
{
    public static AuthenticationResultDto Success(
        Guid userId,
        string identityKey,
        string username,
        string displayName)
    {
        return new AuthenticationResultDto(
            Succeeded: true,
            UserId: userId,
            IdentityKey: identityKey,
            Username: username,
            DisplayName: displayName,
            IsLockedOut: false,
            LockoutUntilUtc: null,
            FailureReason: null);
    }

    public static AuthenticationResultDto Failure(
        string failureReason,
        bool isLockedOut = false,
        DateTimeOffset? lockoutUntilUtc = null)
    {
        return new AuthenticationResultDto(
            Succeeded: false,
            UserId: null,
            IdentityKey: string.Empty,
            Username: string.Empty,
            DisplayName: string.Empty,
            IsLockedOut: isLockedOut,
            LockoutUntilUtc: lockoutUntilUtc,
            FailureReason: failureReason);
    }
}