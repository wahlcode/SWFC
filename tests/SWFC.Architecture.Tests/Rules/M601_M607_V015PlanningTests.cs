using System.Text.Json;
using SWFC.Application.M600_Planning;
using SWFC.Application.M600_Planning.M601_Roadmap;
using SWFC.Application.M600_Planning.M602_Concepts;
using SWFC.Application.M600_Planning.M603_Prototypes;
using SWFC.Application.M600_Planning.M604_Decisions;
using SWFC.Application.M600_Planning.M605_Standards_Guidelines;
using SWFC.Application.M600_Planning.M606_Requirements;
using SWFC.Application.M600_Planning.M607_Evaluation;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M601_M607_V015PlanningTests
{
    [Fact]
    public void V015_Roadmap_Should_Be_Done_With_All_Planning_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.15.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M601", "M602", "M603", "M604", "M605", "M606", "M607" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M601_To_M607_WorkItems_Should_Be_Done_For_V015()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var planningModules = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Where(module => (module.GetProperty("Code").GetString() ?? string.Empty) is
                "M601" or "M602" or "M603" or "M604" or "M605" or "M606" or "M607")
            .ToArray();

        Assert.Equal(7, planningModules.Length);

        foreach (var module in planningModules)
        {
            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void Planning_Catalogs_Should_Record_Changes_And_Links_Without_Operational_Execution()
    {
        var timestamp = new DateTime(2026, 4, 26, 12, 0, 0, DateTimeKind.Utc);
        var requirementCatalog = new RequirementPlanningCatalog();
        var evaluationCatalog = new EvaluationPlanningCatalog();
        var conceptCatalog = new ConceptPlanningCatalog();
        var catalogs = new PlanningCatalog[]
        {
            new RoadmapPlanningCatalog(),
            conceptCatalog,
            new PrototypePlanningCatalog(),
            new DecisionPlanningCatalog(),
            new StandardPlanningCatalog(),
            requirementCatalog,
            evaluationCatalog
        };

        var requirement = requirementCatalog.Add(
            "Support relation fields",
            "Support records need module and object references.",
            PlanningRecordStatus.Draft,
            "planning",
            timestamp,
            "Collected from v0.16.0 support requirement.");

        requirement.LinkTo("M702", "ChangeRequest.ModuleReference", PlanningLinkType.RelatesTo, "planning", timestamp, "Support linkage.");
        requirement.ChangeStatus(PlanningRecordStatus.InReview, "planning", timestamp.AddMinutes(1), "Ready for evaluation.");

        var evaluation = evaluationCatalog.Add(
            "Evaluate support traceability",
            "Check value, risk, effort and dependencies.",
            PlanningRecordStatus.Accepted,
            "planning",
            timestamp.AddMinutes(2),
            "Evaluation completed.");
        evaluation.LinkTo("M606", requirement.Id.ToString(), PlanningLinkType.Evaluates, "planning", timestamp.AddMinutes(3), "Requirement evaluated.");

        Assert.All(catalogs, catalog => Assert.StartsWith("M60", catalog.ModuleCode));
        Assert.Equal(PlanningRecordKind.Requirement, requirement.Kind);
        Assert.Equal(PlanningRecordStatus.InReview, requirement.Status);
        Assert.Single(requirement.Links);
        Assert.Equal(3, requirement.Changes.Count);
        Assert.Equal(PlanningLinkType.Evaluates, evaluation.Links.Single().Type);
    }

    [Fact]
    public void M600_Planning_Modules_Should_Not_Contain_Runtime_Or_Persistence_Logic()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M600-Planning"),
                "*.cs",
                includeGeneratedFiles: false)
            .ToArray();

        Assert.NotEmpty(files);

        var combined = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        Assert.DoesNotContain("TODO", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Placeholder", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Process.Start", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("UseNpgsql", combined, StringComparison.Ordinal);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
