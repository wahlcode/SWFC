using System.Net;
using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;
using SWFC.Architecture.Tests.Support;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;
using SWFC.Infrastructure.M800_Security.Audit;
using System.Text.Json;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M805_M806_RequiredDependencyTests
{
    [Fact]
    public void M805_M806_Status_Should_Match_Verified_Required_Dependency_State()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        foreach (var moduleCode in new[] { "M805", "M806" })
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
    public async Task M805_Audit_Service_Should_Persist_Who_When_What_Context_Result_And_Change_Data()
    {
        var repository = new MemoryAuditRepository();
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        httpContextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        httpContextAccessor.HttpContext.Request.Headers.UserAgent = "Architecture test";

        var service = new AuditService(repository, httpContextAccessor);
        var timestamp = new DateTime(2026, 4, 25, 8, 0, 0, DateTimeKind.Utc);

        await service.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: "user-1",
                ActorDisplayName: "Security Admin",
                Action: "AccessDenied",
                Module: "M806",
                ObjectType: "RuntimeAction",
                ObjectId: "control-1",
                TimestampUtc: timestamp,
                OldValues: "State=Pending",
                NewValues: "State=Denied",
                TargetUserId: "user-2",
                ApprovedByUserId: "approver-1",
                Reason: "Critical action rejected."));

        var entry = Assert.Single(repository.Entries);
        Assert.Equal("user-1", entry.ActorUserId);
        Assert.Equal("Security Admin", entry.ActorDisplayName);
        Assert.Equal("AccessDenied", entry.Action);
        Assert.Equal("M806", entry.Module);
        Assert.Equal("RuntimeAction", entry.ObjectType);
        Assert.Equal("control-1", entry.ObjectId);
        Assert.Equal(timestamp, entry.TimestampUtc);
        Assert.Equal("State=Pending", entry.OldValues);
        Assert.Equal("State=Denied", entry.NewValues);
        Assert.Equal("user-2", entry.TargetUserId);
        Assert.Equal("127.0.0.1", entry.ClientIp);
        Assert.Equal("approver-1", entry.ApprovedByUserId);
        Assert.True(repository.SaveChangesCalled);
        Assert.DoesNotContain(
            typeof(IAuditLogRepository).GetMethods().Select(method => method.Name),
            method => method.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
                      method.Contains("Remove", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task M806_Should_Evaluate_Rbac_Abac_Deny_Context_And_Separate_View_From_Actions()
    {
        var service = new AccessDecisionService();
        var context = new SecurityContext(
            "user-1",
            "identity-1",
            "operator",
            "Operator",
            true,
            roles: new[] { "Operator" },
            permissions: new[] { "runtime.control" },
            permissionModules: new[] { "M504" },
            organizationUnitIds: new[] { "area-a" },
            preferredCultureName: "en-US");

        var visible = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanView,
                "M504",
                "RuntimeScreen",
                "screen-1"));
        var actionWithoutPermission = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanControl,
                "M504",
                "RuntimeAction",
                "control-1"));
        var allowedAction = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanControl,
                "M504",
                "RuntimeAction",
                "control-1",
                requiredPermissions: new[] { "runtime.control" },
                requiredAttributes: new Dictionary<string, string>
                {
                    ["role.Operator"] = "true",
                    ["module.M504"] = "true"
                },
                allowedOrganizationUnitIds: new[] { "AREA-A" }));
        var deniedByAttribute = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanControl,
                "M504",
                "RuntimeAction",
                "control-1",
                requiredPermissions: new[] { "runtime.control" },
                deniedAttributes: new Dictionary<string, string>
                {
                    ["role.Operator"] = "true"
                }));
        var deniedByContext = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanControl,
                "M504",
                "RuntimeAction",
                "control-1",
                requiredPermissions: new[] { "runtime.control" },
                allowedOrganizationUnitIds: new[] { "area-b" }));

        Assert.True(visible.IsAllowed);
        Assert.False(actionWithoutPermission.IsAllowed);
        Assert.Contains("explicit role or permission", actionWithoutPermission.Reason, StringComparison.Ordinal);
        Assert.True(allowedAction.IsAllowed);
        Assert.False(deniedByAttribute.IsAllowed);
        Assert.Contains("Denied attribute", deniedByAttribute.Reason, StringComparison.Ordinal);
        Assert.False(deniedByContext.IsAllowed);
        Assert.Contains("Organization context", deniedByContext.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public async Task M806_Critical_Actions_Should_Require_Approval_And_Emit_Auditable_Result()
    {
        var service = new AccessDecisionService();
        var context = new SecurityContext(
            "user-1",
            "identity-1",
            "operator",
            "Operator",
            true,
            roles: new[] { "Operator" },
            permissions: new[] { "runtime.control" },
            permissionModules: new[] { "M504" });

        var pendingApproval = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanControl,
                "M504",
                "RuntimeAction",
                "control-1",
                requiredPermissions: new[] { "runtime.control" },
                requiresApproval: true));
        var approved = await service.DecideAsync(
            context,
            new AccessDecisionRequest(
                AccessAction.CanControl,
                "M504",
                "RuntimeAction",
                "control-1",
                requiredPermissions: new[] { "runtime.control" },
                requiresApproval: true,
                approvedByUserId: "approver-1"));
        var audit = approved.ToAuditWriteRequest(context, new DateTime(2026, 4, 25, 8, 30, 0, DateTimeKind.Utc));

        Assert.False(pendingApproval.IsAllowed);
        Assert.True(pendingApproval.ApprovalRequired);
        Assert.True(approved.IsAllowed);
        Assert.Equal("AccessAllowed", audit.Action);
        Assert.Equal("M806", audit.Module);
        Assert.Equal("RuntimeAction", audit.ObjectType);
        Assert.Equal("control-1", audit.ObjectId);
        Assert.Equal("approver-1", audit.ApprovedByUserId);
        Assert.Contains("CanControl", audit.NewValues, StringComparison.Ordinal);
    }

    [Fact]
    public async Task M802_Authorization_Should_Use_M806_Decision_Service()
    {
        var authorization = new AuthorizationService(new AccessDecisionService());
        var context = new SecurityContext(
            "user-1",
            "identity-1",
            "planner",
            "Planner",
            true,
            permissions: new[] { "reports.read" });

        var denied = await authorization.AuthorizeAsync(
            context,
            new AuthorizationRequirement(requiredPermissions: new[] { "reports.write" }));
        var allowed = await authorization.AuthorizeAsync(
            context,
            new AuthorizationRequirement(requiredPermissions: new[] { "reports.read" }));

        Assert.False(denied.IsAuthorized);
        Assert.Contains("Missing required permission", denied.Error.Message, StringComparison.Ordinal);
        Assert.True(allowed.IsAuthorized);
    }

    private sealed class MemoryAuditRepository : IAuditLogRepository
    {
        public List<AuditLog> Entries { get; } = [];
        public bool SaveChangesCalled { get; private set; }

        public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            Entries.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AuditLog>> GetRecentAsync(
            int take,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AuditLog>>(Entries.Take(take).ToList());
        }

        public Task<IReadOnlyList<AuditLog>> GetByActorOrTargetUserIdAsync(
            string userId,
            int take,
            CancellationToken cancellationToken = default)
        {
            var entries = Entries
                .Where(entry => entry.ActorUserId == userId || entry.TargetUserId == userId)
                .Take(take)
                .ToList();

            return Task.FromResult<IReadOnlyList<AuditLog>>(entries);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }
}
