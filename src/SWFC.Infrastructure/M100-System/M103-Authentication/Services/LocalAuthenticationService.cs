using Microsoft.Extensions.Options;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.Services;

public sealed class LocalAuthenticationService : ILocalAuthenticationService
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly ILocalCredentialReadRepository _credentialReadRepository;
    private readonly ILocalCredentialWriteRepository _credentialWriteRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly LocalAuthenticationOptions _options;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly M107SetupOptions _initializationOptions;

    public LocalAuthenticationService(
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        ILocalCredentialReadRepository credentialReadRepository,
        ILocalCredentialWriteRepository credentialWriteRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IOptions<AuthenticationOptions> authenticationOptions,
        IOptions<M107SetupOptions> initializationOptions)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _credentialReadRepository = credentialReadRepository;
        _credentialWriteRepository = credentialWriteRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _authenticationOptions = authenticationOptions.Value;
        _initializationOptions = initializationOptions.Value;
        _options = _authenticationOptions.Local ?? new LocalAuthenticationOptions();
    }

    public async Task<AuthenticationResultDto> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResultDto.Failure("Invalid username or password.");
        }

        var normalizedUsername = username.Trim();
        var nowUtc = DateTimeOffset.UtcNow;

        var user = await _userReadRepository.GetByUsernameAsync(normalizedUsername, cancellationToken);

        if (user is null || !user.IsActive)
        {
            await WriteLoginAuditAsync(
                action: "LoginFailed",
                actorUserId: "anonymous",
                actorDisplayName: normalizedUsername,
                objectId: normalizedUsername,
                timestampUtc: nowUtc.UtcDateTime,
                targetUserId: user?.Id.ToString(),
                reason: "Invalid username or password.",
                cancellationToken);

            await _credentialWriteRepository.SaveChangesAsync(cancellationToken);

            return AuthenticationResultDto.Failure("Invalid username or password.");
        }

        var credential = await _credentialWriteRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var isProtectedSuperAdmin = IsProtectedSuperAdmin(user);

        if (credential is null || !credential.IsActive)
        {
            await WriteLoginAuditAsync(
                action: "LoginFailed",
                actorUserId: user.Id.ToString(),
                actorDisplayName: user.DisplayName.Value,
                objectId: user.Id.ToString(),
                timestampUtc: nowUtc.UtcDateTime,
                targetUserId: user.Id.ToString(),
                reason: "Invalid username or password.",
                cancellationToken);

            await _credentialWriteRepository.SaveChangesAsync(cancellationToken);

            return AuthenticationResultDto.Failure("Invalid username or password.");
        }

        if (credential.LockoutUntilUtc.HasValue && credential.LockoutUntilUtc.Value > nowUtc && !isProtectedSuperAdmin)
        {
            await WriteLoginAuditAsync(
                action: "LoginFailed",
                actorUserId: user.Id.ToString(),
                actorDisplayName: user.DisplayName.Value,
                objectId: user.Id.ToString(),
                timestampUtc: nowUtc.UtcDateTime,
                targetUserId: user.Id.ToString(),
                reason: "Account is temporarily locked.",
                cancellationToken);

            await _credentialWriteRepository.SaveChangesAsync(cancellationToken);

            return AuthenticationResultDto.Failure(
                "Account is temporarily locked.",
                isLockedOut: true,
                lockoutUntilUtc: credential.LockoutUntilUtc);
        }

        if (isProtectedSuperAdmin && credential.LockoutUntilUtc.HasValue)
        {
            await _credentialWriteRepository.UpdateAsync(
                credential with
                {
                    FailedAttempts = 0,
                    LockoutUntilUtc = null
                },
                cancellationToken);

            credential = credential with
            {
                FailedAttempts = 0,
                LockoutUntilUtc = null
            };
        }

        if (!_passwordHasher.VerifyPassword(password, credential.PasswordHash))
        {
            var failedAttempts = isProtectedSuperAdmin
                ? 0
                : credential.FailedAttempts + 1;
            DateTimeOffset? lockoutUntilUtc = null;

            if (!isProtectedSuperAdmin && failedAttempts >= _options.MaxFailedAttempts)
            {
                failedAttempts = 0;
                lockoutUntilUtc = nowUtc.AddMinutes(_options.LockoutMinutes);
            }

            await _credentialWriteRepository.UpdateAsync(
                credential with
                {
                    FailedAttempts = failedAttempts,
                    LockoutUntilUtc = lockoutUntilUtc
                },
                cancellationToken);

            await WriteLoginAuditAsync(
                action: "LoginFailed",
                actorUserId: user.Id.ToString(),
                actorDisplayName: user.DisplayName.Value,
                objectId: user.Id.ToString(),
                timestampUtc: nowUtc.UtcDateTime,
                targetUserId: user.Id.ToString(),
                reason: lockoutUntilUtc.HasValue && lockoutUntilUtc.Value > nowUtc
                    ? "Account locked after maximum failed attempts."
                    : isProtectedSuperAdmin
                        ? "Protected SuperAdmin login failed without lockout."
                    : "Invalid username or password.",
                cancellationToken);

            await _credentialWriteRepository.SaveChangesAsync(cancellationToken);

            return lockoutUntilUtc.HasValue && lockoutUntilUtc.Value > nowUtc
                ? AuthenticationResultDto.Failure(
                    "Account is temporarily locked.",
                    isLockedOut: true,
                    lockoutUntilUtc: lockoutUntilUtc)
                : AuthenticationResultDto.Failure("Invalid username or password.");
        }

        await _credentialWriteRepository.UpdateAsync(
            credential with
            {
                FailedAttempts = 0,
                LockoutUntilUtc = null
            },
            cancellationToken);

        await WriteLoginAuditAsync(
            action: "LoginSuccess",
            actorUserId: user.Id.ToString(),
            actorDisplayName: user.DisplayName.Value,
            objectId: user.Id.ToString(),
            timestampUtc: nowUtc.UtcDateTime,
            targetUserId: user.Id.ToString(),
            reason: "Local authentication succeeded.",
            cancellationToken);

        await _credentialWriteRepository.SaveChangesAsync(cancellationToken);

        return AuthenticationResultDto.Success(
            user.Id,
            user.IdentityKey.Value,
            user.Username.Value,
            user.DisplayName.Value);
    }

    public async Task ChangeOwnPasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var credential = await _credentialWriteRepository.GetByUserIdAsync(userId, cancellationToken);

        if (credential is null || !credential.IsActive)
        {
            throw new InvalidOperationException("Local credential not found.");
        }

        if (!_passwordHasher.VerifyPassword(currentPassword, credential.PasswordHash))
        {
            throw new InvalidOperationException("Current password is invalid.");
        }

        await _credentialWriteRepository.UpdateAsync(
            credential with
            {
                PasswordHash = _passwordHasher.HashPassword(newPassword),
                FailedAttempts = 0,
                LockoutUntilUtc = null,
                LastPasswordChangedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);

        await _credentialWriteRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task AdminSetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userWriteRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var credential = await _credentialWriteRepository.GetByUserIdAsync(userId, cancellationToken);
        var nowUtc = DateTimeOffset.UtcNow;
        var newHash = _passwordHasher.HashPassword(newPassword);

        if (credential is null)
        {
            await _credentialWriteRepository.AddAsync(
                new LocalCredentialRecordDto(
                    userId,
                    newHash,
                    true,
                    0,
                    null,
                    nowUtc),
                cancellationToken);
        }
        else
        {
            await _credentialWriteRepository.UpdateAsync(
                credential with
                {
                    PasswordHash = newHash,
                    IsActive = true,
                    FailedAttempts = 0,
                    LockoutUntilUtc = null,
                    LastPasswordChangedAtUtc = nowUtc
                },
                cancellationToken);
        }

        await _credentialWriteRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<LocalCredentialStatusDto?> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var credential = await _credentialReadRepository.GetByUserIdAsync(userId, cancellationToken);

        if (credential is null)
        {
            return null;
        }

        return new LocalCredentialStatusDto(
            credential.UserId,
            credential.IsActive,
            credential.FailedAttempts,
            credential.LockoutUntilUtc,
            credential.LastPasswordChangedAtUtc);
    }

    private Task WriteLoginAuditAsync(
        string action,
        string actorUserId,
        string actorDisplayName,
        string objectId,
        DateTime timestampUtc,
        string? targetUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        return _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actorUserId,
                ActorDisplayName: actorDisplayName,
                Action: action,
                Module: "M103",
                ObjectType: "Authentication",
                ObjectId: objectId,
                TimestampUtc: timestampUtc,
                TargetUserId: targetUserId,
                Reason: reason),
            cancellationToken);
    }

    private bool IsProtectedSuperAdmin(User user)
    {
        return string.Equals(
                   user.IdentityKey.Value,
                   _initializationOptions.SuperAdminIdentityKey,
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   user.Username.Value,
                   _authenticationOptions.InitialSuperAdmin.Username,
                   StringComparison.OrdinalIgnoreCase);
    }
}
