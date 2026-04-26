using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SWFC.Application.M800_Security.M809_CompliancePolicies;

namespace SWFC.Infrastructure.M800_Security.Policies;

public sealed class FileSecurityPolicyRepository : ISecurityPolicyRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _filePath;

    public FileSecurityPolicyRepository(IConfiguration configuration)
    {
        _filePath = configuration["Security:Policies:StorePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "security-store", "policies.json");
    }

    public async Task<SecurityPolicy?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var state = await ReadStateAsync(cancellationToken);

            return state.Policies.LastOrDefault(item =>
                string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(
        SecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var state = await ReadStateAsync(cancellationToken);
            state.Policies.RemoveAll(item =>
                string.Equals(item.Code, policy.Code, StringComparison.OrdinalIgnoreCase));
            state.Policies.Add(policy);
            await WriteStateAsync(state, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<PolicyStoreState> ReadStateAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new PolicyStoreState();
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<PolicyStoreState>(stream, JsonOptions, cancellationToken)
            ?? new PolicyStoreState();
    }

    private async Task WriteStateAsync(
        PolicyStoreState state,
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

    private sealed class PolicyStoreState
    {
        public List<SecurityPolicy> Policies { get; set; } = [];
    }
}
