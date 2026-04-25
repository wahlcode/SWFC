namespace SWFC.Application.M100_System.M103_Authentication;

public interface ILocalAuthenticationService
{
    Task<AuthenticationResultDto> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task ChangeOwnPasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task AdminSetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<LocalCredentialStatusDto?> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface ILocalCredentialReadRepository
{
    Task<LocalCredentialRecordDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface ILocalCredentialWriteRepository
{
    Task<LocalCredentialRecordDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        LocalCredentialRecordDto credential,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        LocalCredentialRecordDto credential,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}

