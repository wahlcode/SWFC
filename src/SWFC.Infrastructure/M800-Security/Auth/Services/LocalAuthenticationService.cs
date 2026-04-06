using Microsoft.Extensions.Options;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M103_Authentication.DTOs;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;

namespace SWFC.Infrastructure.M800_Security.Auth.Services;

public sealed class LocalAuthenticationService : ILocalAuthenticationService
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly ILocalCredentialReadRepository _credentialReadRepository;
    private readonly ILocalCredentialWriteRepository _credentialWriteRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly LocalAuthenticationOptions _options;

    public LocalAuthenticationService(
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        ILocalCredentialReadRepository credentialReadRepository,
        ILocalCredentialWriteRepository credentialWriteRepository,
        IPasswordHasher passwordHasher,
        IOptions<AuthenticationOptions> authenticationOptions)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _credentialReadRepository = credentialReadRepository;
        _credentialWriteRepository = credentialWriteRepository;
        _passwordHasher = passwordHasher;
        _options = authenticationOptions.Value.Local ?? new LocalAuthenticationOptions();
    }

    public async Task<AuthenticationResultDto> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();
        var nowUtc = DateTimeOffset.UtcNow;

        var user = await _userReadRepository.GetByUsernameAsync(normalizedUsername, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AuthenticationResultDto.Failure("Invalid username or password.");
        }

        var credential = await _credentialWriteRepository.GetByUserIdAsync(user.Id, cancellationToken);

        Console.WriteLine($"LOGIN USERNAME: '{normalizedUsername}'");
        Console.WriteLine($"DB USERNAME: '{user.Username.Value}'");
        Console.WriteLine($"VERIFY RESULT: {_passwordHasher.VerifyPassword(password, credential?.PasswordHash ?? string.Empty)}");
        Console.WriteLine($"INPUT PASSWORD: '{password}'");
        Console.WriteLine($"STORED HASH: '{credential?.PasswordHash}'");

        if (credential is null || !credential.IsActive)
        {
            return AuthenticationResultDto.Failure("Invalid username or password.");
        }

        if (credential.LockoutUntilUtc.HasValue && credential.LockoutUntilUtc.Value > nowUtc)
        {
            return AuthenticationResultDto.Failure(
                "Account is temporarily locked.",
                isLockedOut: true,
                lockoutUntilUtc: credential.LockoutUntilUtc);
        }

        if (!_passwordHasher.VerifyPassword(password, credential.PasswordHash))
        {
            var failedAttempts = credential.FailedAttempts + 1;
            DateTimeOffset? lockoutUntilUtc = credential.LockoutUntilUtc;

            if (failedAttempts >= _options.MaxFailedAttempts)
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
}