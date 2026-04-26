using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Domain.M700_Support.M702_ChangeRequests;
using SWFC.Domain.M700_Support.M703_SupportCases;
using SWFC.Domain.M700_Support.M704_Incident_Management;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M701_M706_V016SupportTests
{
    [Fact]
    public void V016_Roadmap_Should_Be_Done_With_All_Support_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.16.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M701", "M702", "M703", "M704", "M705", "M706" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M701_To_M706_WorkItems_Should_Be_Done_For_V016()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var supportModules = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Where(module => (module.GetProperty("Code").GetString() ?? string.Empty) is
                "M701" or "M702" or "M703" or "M704" or "M705" or "M706")
            .ToArray();

        Assert.Equal(6, supportModules.Length);

        foreach (var module in supportModules)
        {
            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void Support_Records_Should_Capture_Status_History_And_Module_Object_Links()
    {
        var createContext = ChangeContext.Create("support-user", "Create structured support records.");
        var updateContext = ChangeContext.Create("support-user", "Update status for traceability.");

        var bug = Bug.Create(
            "Pump dashboard crashes.",
            "Open M201 dashboard and filter pump assets.",
            BugStatus.Open,
            createContext,
            "M201",
            "Machine:Pump-01");
        bug.UpdateDetails(
            bug.Description,
            bug.Reproducibility,
            BugStatus.InProgress,
            updateContext,
            "M201",
            "Machine:Pump-01");

        var changeRequest = ChangeRequest.Create(
            ChangeRequestType.Extension,
            "Add SLA relation to support case.",
            "REQ-100",
            "v0.16.0",
            createContext,
            ChangeRequestStatus.InReview,
            "M706",
            "SLA:Critical");

        var supportCase = SupportCase.Create(
            "Cannot confirm incident handover.",
            "Operator sees missing confirmation state.",
            createContext,
            SupportCaseStatus.InProgress,
            "M704",
            "Incident:INC-100");
        supportCase.LinkBug(bug.Id, updateContext);

        var incident = Incident.Create(
            IncidentCategory.SystemOutage,
            "Support portal unavailable.",
            "Escalate to on-call.",
            "Manual communication active.",
            "M303:Notification:100",
            "M500:Run:100",
            createContext,
            IncidentStatus.Escalated,
            "M700",
            "SupportPortal");

        var knowledge = KnowledgeEntry.Create(
            KnowledgeEntryType.Instruction,
            "Restart support portal app service and verify health endpoint.",
            createContext,
            KnowledgeEntryStatus.Published,
            "M704",
            "Incident:SystemOutage");

        var serviceLevel = ServiceLevel.Create(
            "Critical",
            TimeSpan.FromMinutes(15),
            TimeSpan.FromHours(4),
            useForSupport: true,
            useForIncidentManagement: true,
            createContext,
            ServiceLevelStatus.Active,
            "M703",
            "SupportCase:Critical");

        Assert.Equal(BugStatus.InProgress, bug.Status);
        Assert.Contains("Updated|InProgress", bug.HistoryLog);
        Assert.Equal("M201", bug.ModuleReference);
        Assert.Equal("Machine:Pump-01", bug.ObjectReference);
        Assert.Equal(ChangeRequestStatus.InReview, changeRequest.Status);
        Assert.Equal(bug.Id, supportCase.TriggeredBugId);
        Assert.Contains("LinkedBug", supportCase.HistoryLog);
        Assert.Equal(IncidentStatus.Escalated, incident.Status);
        Assert.Equal(KnowledgeEntryStatus.Published, knowledge.Status);
        Assert.Equal(ServiceLevelStatus.Active, serviceLevel.Status);
    }

    [Fact]
    public void M700_Support_Domain_Should_Not_Contain_Hidden_Process_Automation_Or_Placeholders()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Domain", "M700-Support"),
                "*.cs",
                includeGeneratedFiles: false)
            .ToArray();

        Assert.NotEmpty(files);

        var combined = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        Assert.DoesNotContain("TODO", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Placeholder", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Scheduler", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Automation", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combined, StringComparison.Ordinal);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
