using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M803_DataSecurity;
using SWFC.Application.M800_Security.M804_DevSecOps;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;
using SWFC.Application.M800_Security.M807_SecretsKeyManagement;
using SWFC.Application.M800_Security.M808_SecurityMonitoring;
using SWFC.Application.M800_Security.M809_CompliancePolicies;
using SWFC.Architecture.Tests.Support;
using System.Text.Json;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M803_M809_V017SecurityCompletionTests
{
    [Fact]
    public void V017_Roadmap_And_Security_Modules_Should_Be_Marked_Done_After_Verification()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var v017 = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(version => version.GetProperty("Version").GetString() == "v0.17.0");

        Assert.Equal("Done", v017.GetProperty("Status").GetString());
        Assert.All(
            v017.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));

        foreach (var moduleCode in new[] { "M801", "M802", "M803", "M804", "M805", "M806", "M807", "M808", "M809" })
        {
            var module = modules.RootElement
                .GetProperty("Groups")
                .EnumerateArray()
                .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
                .Single(item => item.GetProperty("Code").GetString() == moduleCode);

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                workItem => Assert.Equal("Done", workItem.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public async Task M803_DataProtection_Should_Require_Access_Protect_Data_And_Audit()
    {
        var audit = new MemoryAuditService();
        var service = new DataProtectionService(
            new AccessDecisionService(),
            new ReversibleTestProtector(),
            audit);
        var allowed = new SecurityContext(
            "user-1",
            "identity-1",
            "security",
            "Security",
            true,
            permissions: new[] { "data.protect", "data.reveal" },
            permissionModules: new[] { "M803" });

        var protectedResult = await service.ProtectAsync(
            allowed,
            new DataProtectionRequest(
                "M803",
                "PersonalData",
                "person-1",
                "NationalId",
                "12345",
                SensitiveDataClassification.Confidential,
                "Compliance storage"));

        Assert.True(protectedResult.IsSuccess);
        Assert.NotEqual("12345", protectedResult.Value!.CipherText);

        var denied = await service.RevealAsync(
            new SecurityContext("user-2", "identity-2", "viewer", "Viewer", true),
            "M803",
            "PersonalData",
            "person-1",
            protectedResult.Value,
            "No permission");

        Assert.True(denied.IsFailure);
        Assert.Contains(audit.Requests, entry => entry.Action == "AccessDenied" && entry.Module == "M806");
        Assert.Contains(audit.Requests, entry => entry.Action == "DataProtected" && entry.Module == "M803");
    }

    [Fact]
    public async Task M807_Secrets_Should_Not_Store_ClearText_Should_Enforce_Access_And_Audit_Rotation()
    {
        var repository = new MemorySecretRepository();
        var audit = new MemoryAuditService();
        var service = new SecretVaultService(
            repository,
            new ReversibleTestProtector(),
            new AccessDecisionService(),
            audit);
        var actor = new SecurityContext(
            "user-1",
            "identity-1",
            "security",
            "Security",
            true,
            permissions: new[] { "security.secrets.write", "security.secrets.read" },
            permissionModules: new[] { "M807" });

        var stored = await service.StoreAsync(actor, "erp-api-key", SecretKind.ApiKey, "clear-secret", "Initial import");
        var rotated = await service.RotateAsync(actor, "erp-api-key", "rotated-secret", "Key rotation");
        var retrieved = await service.RetrieveAsync(actor, "erp-api-key", "Integration call");

        Assert.True(stored.IsSuccess);
        Assert.True(rotated.IsSuccess);
        Assert.True(retrieved.IsSuccess);
        Assert.Equal(2, rotated.Value!.Version);
        Assert.Equal("rotated-secret", retrieved.Value);
        Assert.DoesNotContain(repository.Secrets, item => item.ProtectedValue.CipherText == "clear-secret");
        Assert.Contains(audit.Requests, entry => entry.Action == "SecretRotated" && entry.Module == "M807");

        var denied = await service.RetrieveAsync(
            new SecurityContext("user-2", "identity-2", "viewer", "Viewer", true),
            "erp-api-key",
            "No permission");

        Assert.True(denied.IsFailure);
        Assert.Contains(audit.Requests, entry => entry.Action == "AccessDenied" && entry.Module == "M806");
    }

    [Fact]
    public async Task M808_Monitoring_Should_Raise_And_Audit_Critical_Security_Alerts()
    {
        var audit = new MemoryAuditService();
        var service = new SecurityMonitoringService(audit);
        var now = new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc);
        var events = Enumerable.Range(0, 5)
            .Select(index => new SecurityMonitoringEvent(
                SecurityEventKind.LoginFailed,
                "user-1",
                "127.0.0.1",
                now.AddMinutes(index),
                "Login",
                "local"))
            .ToArray();

        var alerts = await service.AnalyzeAsync(events);

        Assert.Contains(alerts, alert => alert.Code == "m808.login.failed.repeated" && alert.Severity == SecurityAlertSeverity.Critical);
        Assert.Contains(audit.Requests, entry => entry.Action == "SecurityAlertRaised" && entry.Module == "M808");
    }

    [Fact]
    public async Task M809_Policies_Should_Block_Unauthorized_Changes_And_Audit_Violations()
    {
        var repository = new MemoryPolicyRepository();
        var audit = new MemoryAuditService();
        var service = new SecurityPolicyService(repository, new AccessDecisionService(), audit);
        var policy = new SecurityPolicy(
            "password.minimum",
            "Password minimum rules",
            "M103",
            true,
            new Dictionary<string, string>
            {
                ["minLength"] = "12",
                ["requiresMfa"] = "true"
            });

        var denied = await service.SavePolicyAsync(
            new SecurityContext("user-2", "identity-2", "viewer", "Viewer", true),
            policy,
            "Unauthorized change");

        var allowed = await service.SavePolicyAsync(
            new SecurityContext(
                "user-1",
                "identity-1",
                "security",
                "Security",
                true,
                permissions: new[] { "security.policies.write" },
                permissionModules: new[] { "M809" }),
            policy,
            "Baseline policy");

        var evaluation = await service.EvaluateAsync(
            new SecurityContext("user-3", "identity-3", "auditor", "Auditor", true),
            new PolicyEvaluationRequest(
                "password.minimum",
                "M103",
                "PasswordChange",
                "user-3",
                new Dictionary<string, string> { ["minLength"] = "8", ["requiresMfa"] = "true" },
                "Password policy check"));

        Assert.True(denied.IsFailure);
        Assert.True(allowed.IsSuccess);
        Assert.True(evaluation.IsSuccess);
        Assert.False(evaluation.Value!.IsCompliant);
        Assert.Contains("minLength must be 12", evaluation.Value.Violations);
        Assert.Contains(audit.Requests, entry => entry.Action == "PolicyViolation" && entry.Module == "M809");
    }

    [Fact]
    public void M804_ReleaseGate_Should_Block_Unsigned_Or_Unchecked_Deployments()
    {
        var gate = new SecurityReleaseGate();

        var blocked = gate.Verify(new SecurityReleaseEvidence(
            "build-1",
            BuildSigned: false,
            SourceReviewed: true,
            DependencyScanPassed: true,
            SecurityTestsPassed: true,
            DeploymentApproved: true));
        var allowed = gate.Verify(new SecurityReleaseEvidence(
            "build-2",
            BuildSigned: true,
            SourceReviewed: true,
            DependencyScanPassed: true,
            SecurityTestsPassed: true,
            DeploymentApproved: true));

        Assert.True(blocked.IsFailure);
        Assert.Contains("signed build", blocked.Error.Message);
        Assert.True(allowed.IsSuccess);
    }

    [Fact]
    public async Task M806_Should_Support_Inherited_Wildcard_Permissions_And_Auditable_Approval()
    {
        var service = new AccessDecisionService();
        var context = new SecurityContext(
            "user-1",
            "identity-1",
            "security",
            "Security",
            true,
            permissions: new[] { "security.*" },
            permissionModules: new[] { "M809" });

        var decision = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanAdminister,
                "M809",
                "SecurityPolicy",
                "password.minimum",
                requiredPermissions: new[] { "security.policies.write" },
                requiresApproval: true,
                approvedByUserId: "approver-1"));
        var audit = decision.ToAuditWriteRequest(context, new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc));

        Assert.True(decision.IsAllowed);
        Assert.Equal("AccessAllowed", audit.Action);
        Assert.Equal("approver-1", audit.ApprovedByUserId);
    }

    private sealed class ReversibleTestProtector : ISensitiveDataProtector
    {
        public ProtectedDataPayload Protect(string plainText, SensitiveDataClassification classification)
        {
            return new ProtectedDataPayload(
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText)),
                "test-reversible",
                "test",
                classification,
                DateTime.UtcNow);
        }

        public string Reveal(ProtectedDataPayload payload)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload.CipherText));
        }
    }

    private sealed class MemoryAuditService : IAuditService
    {
        public List<AuditWriteRequest> Requests { get; } = [];

        public Task WriteAsync(
            string userId,
            string username,
            string action,
            string entity,
            string entityId,
            DateTime timestampUtc,
            string? oldValues = null,
            string? newValues = null,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(new AuditWriteRequest(
                userId,
                username,
                action,
                "General",
                entity,
                entityId,
                timestampUtc,
                oldValues,
                newValues));

            return Task.CompletedTask;
        }

        public Task WriteAsync(
            AuditWriteRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.CompletedTask;
        }
    }

    private sealed class MemorySecretRepository : ISecretVaultRepository
    {
        public List<StoredSecret> Secrets { get; } = [];

        public Task<StoredSecret?> GetActiveByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Secrets.LastOrDefault(item =>
                item.IsActive &&
                string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)));
        }

        public Task AddAsync(
            StoredSecret secret,
            CancellationToken cancellationToken = default)
        {
            Secrets.Add(secret);
            return Task.CompletedTask;
        }

        public Task DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var index = Secrets.FindIndex(item => item.Id == id);

            if (index >= 0)
            {
                var current = Secrets[index];
                Secrets[index] = current with { IsActive = false };
            }

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class MemoryPolicyRepository : ISecurityPolicyRepository
    {
        private readonly Dictionary<string, SecurityPolicy> _policies = new(StringComparer.OrdinalIgnoreCase);

        public Task<SecurityPolicy?> GetByCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            _policies.TryGetValue(code, out var policy);
            return Task.FromResult(policy);
        }

        public Task SaveAsync(
            SecurityPolicy policy,
            CancellationToken cancellationToken = default)
        {
            _policies[policy.Code] = policy;
            return Task.CompletedTask;
        }
    }
}
