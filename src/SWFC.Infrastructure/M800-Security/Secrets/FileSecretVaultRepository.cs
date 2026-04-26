using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SWFC.Application.M800_Security.M803_DataSecurity;
using SWFC.Application.M800_Security.M807_SecretsKeyManagement;

namespace SWFC.Infrastructure.M800_Security.Secrets;

public sealed class FileSecretVaultRepository : ISecretVaultRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _filePath;

    public FileSecretVaultRepository(IConfiguration configuration)
    {
        _filePath = configuration["Security:Secrets:StorePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "security-store", "secrets.json");
    }

    public async Task<StoredSecret?> GetActiveByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var state = await ReadStateAsync(cancellationToken);

            return state.Secrets
                .Where(item => item.IsActive)
                .LastOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                ?.ToStoredSecret();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(
        StoredSecret secret,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var state = await ReadStateAsync(cancellationToken);
            state.Secrets.Add(SecretRecord.From(secret));
            await WriteStateAsync(state, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var state = await ReadStateAsync(cancellationToken);
            var record = state.Secrets.FirstOrDefault(item => item.Id == id);

            if (record is not null)
            {
                record.IsActive = false;
                await WriteStateAsync(state, cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    private async Task<SecretStoreState> ReadStateAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new SecretStoreState();
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<SecretStoreState>(stream, JsonOptions, cancellationToken)
            ?? new SecretStoreState();
    }

    private async Task WriteStateAsync(
        SecretStoreState state,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
    }

    private sealed class SecretStoreState
    {
        public List<SecretRecord> Secrets { get; set; } = [];
    }

    private sealed class SecretRecord
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SecretKind Kind { get; set; }
        public int Version { get; set; }
        public string CipherText { get; set; } = string.Empty;
        public string ProtectionScheme { get; set; } = string.Empty;
        public string KeyVersion { get; set; } = string.Empty;
        public SensitiveDataClassification Classification { get; set; }
        public DateTime ProtectedAtUtc { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public static SecretRecord From(StoredSecret secret) =>
            new()
            {
                Id = secret.Id,
                Name = secret.Name,
                Kind = secret.Kind,
                Version = secret.Version,
                CipherText = secret.ProtectedValue.CipherText,
                ProtectionScheme = secret.ProtectedValue.ProtectionScheme,
                KeyVersion = secret.ProtectedValue.KeyVersion,
                Classification = secret.ProtectedValue.Classification,
                ProtectedAtUtc = secret.ProtectedValue.ProtectedAtUtc,
                IsActive = secret.IsActive,
                CreatedAtUtc = secret.CreatedAtUtc
            };

        public StoredSecret ToStoredSecret() =>
            new(
                Id,
                Name,
                Kind,
                Version,
                new ProtectedDataPayload(
                    CipherText,
                    ProtectionScheme,
                    KeyVersion,
                    Classification,
                    ProtectedAtUtc),
                IsActive,
                CreatedAtUtc);
    }
}
