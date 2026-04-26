using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Application.M200_Business.M209_Projects;
using SWFC.Application.M200_Business.M210_Customers;
using SWFC.Application.M200_Business.M212_Production;
using SWFC.Application.M200_Business.M213_Workforce;
using SWFC.Domain.M200_Business.M209_Projects;
using SWFC.Domain.M200_Business.M210_Customers;
using SWFC.Domain.M200_Business.M212_Production;
using SWFC.Domain.M200_Business.M213_Workforce;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M209_M210_M212_M213_V011CompletionTests
{
    [Fact]
    public void V011_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.11.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M209", "M210", "M212", "M213" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void V011_Modules_Should_Have_All_WorkItems_Done()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        foreach (var moduleCode in new[] { "M209", "M210", "M212", "M213" })
        {
            var module = FindModule(modules, moduleCode);

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void V011_Domain_Should_Model_Operations_Without_Delete_Workflows()
    {
        var project = new Project(Guid.NewGuid(), "KVP Projekt", Guid.NewGuid(), new DateOnly(2026, 4, 1), new DateOnly(2026, 5, 1));
        project.LinkReference(new ProjectReference("M207", Guid.NewGuid(), "Qualitaetsmassnahme"));
        project.LinkReference(new ProjectReference("M202", Guid.NewGuid(), "Wartungsumbau"));
        project.LinkReference(new ProjectReference("M201", Guid.NewGuid(), "Maschine"));
        project.AddMaterial(new ProjectMaterialReference(Guid.NewGuid(), 2, "pcs"));
        var measure = project.AddMeasure("Massnahme", Guid.NewGuid(), new DateOnly(2026, 4, 2), new DateOnly(2026, 4, 15));
        measure.AddTask("Aufgabe", Guid.NewGuid(), new DateOnly(2026, 4, 10));

        var customer = new Customer(Guid.NewGuid(), "Kunde", "ERP-42");
        customer.AddContact(new CustomerContact("Kontakt", "Betrieb", "mail@example.local"));
        customer.Assign(new CustomerAssignment(CustomerAssignmentType.Project, project.Id, "KVP Projekt"));

        var production = new ProductionOrder(Guid.NewGuid(), "PO-1", Guid.NewGuid(), "Produkt", 100, "pcs");
        production.Start();
        production.AddFeedback(new ProductionFeedback(
            production.MachineId,
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow,
            MachineRuntimeState.Running,
            20,
            1,
            "pcs",
            null));

        var assignment = new WorkforceAssignment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new WorkforceTarget("M209", project.Id, "Aufgabe"),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(2),
            "Projektarbeit");
        assignment.Start();
        assignment.AddFeedback(new ActivityFeedback(
            DateTimeOffset.UtcNow.AddHours(-2),
            DateTimeOffset.UtcNow.AddHours(-1),
            "erledigt"));

        Assert.Single(project.Materials);
        Assert.Equal(3, project.References.Count);
        Assert.True(customer.IsActive);
        Assert.True(production.Feedback.Single().CanTriggerQualityCase);
        Assert.True(production.Feedback.Single().CanCorrelateEnergy);
        Assert.Equal(WorkforceAssignmentStatus.Completed, assignment.Status);

        var deleteUseCases = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M200-Business"),
                "Delete*.cs",
                includeGeneratedFiles: false)
            .Where(path =>
                path.Contains("M209-Projects", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("M210-Customers", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("M212-Production", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("M213-Workforce", StringComparison.OrdinalIgnoreCase))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(deleteUseCases.Length == 0, $"V0.11 modules must historize instead of delete: {string.Join(", ", deleteUseCases)}");
    }

    [Fact]
    public void V011_Integration_Should_Have_Reachable_Services_Ui_And_Navigation()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M209-Projects", "ProjectWorkspaceService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M210-Customers", "CustomerWorkspaceService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M212-Production", "ProductionWorkspaceService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M213-Workforce", "WorkforceWorkspaceService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M209-Projects", "Projects.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M210-Customers", "Customers.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M212-Production", "Production.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M213-Workforce", "Workforce.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Program.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("/m200/projects", combinedContent, StringComparison.Ordinal);
        Assert.Contains("/m200/customers", combinedContent, StringComparison.Ordinal);
        Assert.Contains("/m200/production", combinedContent, StringComparison.Ordinal);
        Assert.Contains("/m200/workforce", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AddSingleton<ProjectWorkspaceService>", combinedContent, StringComparison.Ordinal);
        Assert.Contains("ProjectMaterialReference", combinedContent, StringComparison.Ordinal);
        Assert.Contains("CustomerAssignmentType.QualityCase", combinedContent, StringComparison.Ordinal);
        Assert.Contains("CanCorrelateEnergy", combinedContent, StringComparison.Ordinal);
        Assert.Contains("ShiftModelId", combinedContent, StringComparison.Ordinal);
    }

    private static JsonElement FindModule(JsonDocument modules, string moduleCode)
    {
        return modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == moduleCode);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
