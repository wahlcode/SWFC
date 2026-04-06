namespace SWFC.Application.M100_System.M103_Authentication.DTOs;

public sealed record LocalCredentialRecordDto(
    Guid UserId,
    string PasswordHash,
    bool IsActive,
    int FailedAttempts,
    DateTimeOffset? LockoutUntilUtc,
    DateTimeOffset LastPasswordChangedAtUtc);