using SWFC.Domain.M200_Business.M209_Projects;

namespace SWFC.Application.M200_Business.M209_Projects;

public sealed record ProjectSummaryDto(
    Guid Id,
    string Title,
    string Status,
    DateOnly PlannedStart,
    DateOnly? PlannedEnd,
    int MeasureCount,
    int TaskCount,
    int MaterialReferenceCount,
    int LinkedReferenceCount);

public sealed class ProjectWorkspaceService
{
    private readonly List<Project> _projects = new();

    public ProjectWorkspaceService()
    {
        SeedOperationalProject();
    }

    public IReadOnlyList<ProjectSummaryDto> GetProjects()
    {
        return _projects
            .Select(project => new ProjectSummaryDto(
                project.Id,
                project.Title,
                project.Status.ToString(),
                project.PlannedStart,
                project.PlannedEnd,
                project.Measures.Count,
                project.Measures.Sum(x => x.Tasks.Count),
                project.Materials.Count,
                project.References.Count))
            .ToList();
    }

    public Project CreateProject(string title, Guid organizationUnitId, DateOnly plannedStart, DateOnly? plannedEnd)
    {
        var project = new Project(Guid.NewGuid(), title, organizationUnitId, plannedStart, plannedEnd);
        _projects.Add(project);
        return project;
    }

    private void SeedOperationalProject()
    {
        var project = new Project(
            Guid.Parse("20900000-0000-0000-0000-000000000001"),
            "KVP Energie und Wartung",
            Guid.Parse("10200000-0000-0000-0000-000000000001"),
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 6, 30));
        project.LinkReference(new ProjectReference("M207", Guid.Parse("20700000-0000-0000-0000-000000000001"), "Qualitaetsmassnahme"));
        project.LinkReference(new ProjectReference("M202", Guid.Parse("20200000-0000-0000-0000-000000000001"), "Umbau-Wartungsauftrag"));
        project.LinkReference(new ProjectReference("M201", Guid.Parse("20100000-0000-0000-0000-000000000001"), "Maschine A-01"));
        project.AddMaterial(new ProjectMaterialReference(Guid.Parse("20400000-0000-0000-0000-000000000001"), 4, "pcs"));
        project.Start();

        var measure = project.AddMeasure(
            "Druckluftleckagen reduzieren",
            Guid.Parse("10200000-0000-0000-0000-000000000002"),
            new DateOnly(2026, 4, 15),
            new DateOnly(2026, 5, 15));
        measure.Start();
        measure.AddTask("Leckageliste pruefen", Guid.Parse("10200000-0000-0000-0000-000000000003"), new DateOnly(2026, 4, 30));

        _projects.Add(project);
    }
}
