namespace SWFC.Application.M100_System.M103_Authentication.DTOs;

public sealed record LocalCredentialStatusDto(
    Guid UserId,
    bool IsActive,
    int FailedAttempts,
    DateTimeOffset? LockoutUntilUtc,
    DateTimeOffset? LastPasswordChangedAtUtc);