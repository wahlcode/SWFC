using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Users.Delegations;
using SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M102_V020CompletionTests
{
    [Fact]
    public void V020_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.2.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M102", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M101", "M103", "M805", "M806" },
            required => Assert.Contains(required, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M102_WorkItems_Should_Be_Done_For_V020()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m102 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M102");

        var v020Items = m102.GetProperty("WorkItems").EnumerateArray().ToArray();

        Assert.NotEmpty(v020Items);
        Assert.All(v020Items, item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        Assert.Equal("Full Complete", m102.GetProperty("Status").GetString());
        Assert.Equal(100, m102.GetProperty("ProgressPercent").GetInt32());
    }

    [Fact]
    public void M102_Implementation_Should_Cover_Organization_User_CostCenter_And_Shift_Milestones()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M100-System", "M102-Organization", "OrganizationUnits", "OrganizationUnit.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "OrganizationUnits", "CreateOrganizationUnit.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "OrganizationUnits", "UpdateOrganizationUnit.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M102-Organization", "OrganizationUnits", "OrganizationUnits.razor"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M100-System", "M102-Organization", "Users", "User.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "Users", "CreateUser.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "Users", "UpdateUser.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "Users", "ChangeUserStatus.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M102-Organization", "Users", "Users.razor"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M100-System", "M102-Organization", "CostCenters", "CostCenter.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M102-Organization", "CostCenters", "CostCenters.razor"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M100-System", "M102-Organization", "ShiftModels", "ShiftModel.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M102-Organization", "ShiftModels", "ShiftModels.razor")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var organizationUpdate = File.ReadAllText(requiredFiles[2]);
        Assert.Contains("CreatesCycleAsync", organizationUpdate, StringComparison.Ordinal);
        Assert.Contains("organization.write", organizationUpdate, StringComparison.Ordinal);
        Assert.Contains("IAuditService", organizationUpdate, StringComparison.Ordinal);

        var userCreate = File.ReadAllText(requiredFiles[5]);
        Assert.Contains("PrimaryOrganizationUnitId", userCreate, StringComparison.Ordinal);
        Assert.Contains("CostCenterId", userCreate, StringComparison.Ordinal);
        Assert.Contains("ShiftModelId", userCreate, StringComparison.Ordinal);
        Assert.Contains("UserHistoryEntry.Create", userCreate, StringComparison.Ordinal);
        Assert.Contains("IAuditService", userCreate, StringComparison.Ordinal);
    }

    [Fact]
    public void M102_Extension_Items_Should_Have_Runtime_Behavior_And_Pipeline_Artifacts()
    {
        var changeContext = ChangeContext.Create("user-1", "Audit reason");
        var organizationUnitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var delegateUserId = Guid.NewGuid();
        var validFrom = new DateTime(2026, 4, 25, 8, 0, 0, DateTimeKind.Utc);

        var externalPerson = ExternalPerson.Create(
            "Service Partner",
            "Partner Company",
            "partner@example.test",
            "+49 123",
            "Maintenance",
            organizationUnitId,
            changeContext);
        var delegation = UserDelegation.Create(
            userId,
            delegateUserId,
            "Escalation",
            validFrom,
            validFrom.AddDays(7),
            changeContext);

        Assert.True(externalPerson.IsActive);
        Assert.Equal("Service Partner", externalPerson.DisplayName);
        Assert.Equal(organizationUnitId, externalPerson.OrganizationUnitId);
        Assert.True(delegation.IsActive);
        Assert.Equal(userId, delegation.UserId);
        Assert.Equal(delegateUserId, delegation.DelegateUserId);
        Assert.Throws<ArgumentException>(() => UserDelegation.Create(
            userId,
            userId,
            "Escalation",
            validFrom,
            validFrom.AddDays(1),
            changeContext));

        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "Users", "ExternalPersons", "CreateExternalPerson.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M100-System", "M102-Organization", "Users", "Delegations", "CreateUserDelegation.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Repositories", "M100-System", "ExternalPersonWriteRepository.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Repositories", "M100-System", "UserDelegationWriteRepository.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "DependencyInjection", "ServiceCollectionExtensions.cs")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("CreateExternalPersonCommand", combinedContent, StringComparison.Ordinal);
        Assert.Contains("CreateUserDelegationCommand", combinedContent, StringComparison.Ordinal);
        Assert.Contains("IAuditService", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AddScoped<IExternalPersonWriteRepository", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AddScoped<IUserDelegationWriteRepository", combinedContent, StringComparison.Ordinal);
    }

    [Fact]
    public void M102_Web_Ui_Should_Not_Contain_German_User_Facing_Text()
    {
        var forbiddenTerms = new[]
        {
            "Speichern",
            "Abbrechen",
            "Zurueck",
            "Zurück",
            "Begruendung",
            "Begründung",
            "Organisationseinheit",
            "Kostenstelle",
            "Schichtmodell",
            "Benutzer",
            "Keine",
            "Lade"
        };

        var files = Directory
            .EnumerateFiles(
                RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M102-Organization"),
                "*.*",
                SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);

            foreach (var term in forbiddenTerms)
            {
                Assert.DoesNotContain(term, content, StringComparison.Ordinal);
            }
        }
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
