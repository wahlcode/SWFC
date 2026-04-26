using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M803_DataSecurity;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M807_SecretsKeyManagement;

public enum SecretKind
{
    ApiKey = 1,
    Token = 2,
    Certificate = 3,
    EncryptionKey = 4
}

public sealed record SecretDescriptor(
    Guid Id,
    string Name,
    SecretKind Kind,
    int Version,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record StoredSecret(
    Guid Id,
    string Name,
    SecretKind Kind,
    int Version,
    ProtectedDataPayload ProtectedValue,
    bool IsActive,
    DateTime CreatedAtUtc);

public interface ISecretVaultRepository
{
    Task<StoredSecret?> GetActiveByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoredSecret secret,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ISecretVaultService
{
    Task<Result<SecretDescriptor>> StoreAsync(
        SecurityContext actor,
        string name,
        SecretKind kind,
        string plainTextValue,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result<string>> RetrieveAsync(
        SecurityContext actor,
        string name,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result<SecretDescriptor>> RotateAsync(
        SecurityContext actor,
        string name,
        string newPlainTextValue,
        string reason,
        CancellationToken cancellationToken = default);
}
