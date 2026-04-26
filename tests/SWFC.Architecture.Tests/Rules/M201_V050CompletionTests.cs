using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Hierarchy;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponents;
using SWFC.Domain.M200_Business.M201_Assets.Machines;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M201_V050CompletionTests
{
    [Fact]
    public void V050_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.5.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M201", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M101", "M102", "M301", "M806" },
            required => Assert.Contains(required, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M201_WorkItems_Should_Be_Done_For_V050()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m201 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M201");

        Assert.Equal("Full Complete", m201.GetProperty("Status").GetString());
        Assert.Equal(100, m201.GetProperty("ProgressPercent").GetInt32());
        Assert.All(
            m201.GetProperty("WorkItems").EnumerateArray(),
            item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M201_Domain_Should_Enforce_Tree_Deactivation_And_Type_References()
    {
        var changeContext = ChangeContext.Create("tester", "v0.5 verification");
        var energyObjectId = Guid.NewGuid();
        var machine = CreateMachine(changeContext, energyObjectId);

        Assert.Equal("Production line", machine.AssetType.Value);
        Assert.Equal(energyObjectId, machine.EnergyObjectId);
        Assert.False(MachineHierarchyRules.CanAssignParent(
            machine.Id,
            machine.Id,
            Array.Empty<Guid>()));
        var descendantId = Guid.NewGuid();
        Assert.False(MachineHierarchyRules.CanAssignParent(
            machine.Id,
            descendantId,
            new[] { descendantId }));

        var parentComponent = MachineComponent.Create(
            machine.Id,
            null,
            null,
            MachineComponentName.Create("Drive"),
            MachineComponentDescription.Create("Drive assembly"),
            changeContext);
        var component = MachineComponent.Create(
            machine.Id,
            null,
            parentComponent,
            MachineComponentName.Create("Motor"),
            MachineComponentDescription.Create("Motor instance"),
            changeContext);

        Assert.Equal(parentComponent.Id, component.ParentMachineComponentId);
        Assert.Throws<DomainException>(() => component.SetActiveState(false, hasChildren: true, changeContext));
        Assert.False(MachineComponentHierarchyRules.CanAssignParent(
            component.Id,
            machine.Id,
            parentComponent.Id,
            Guid.NewGuid(),
            Array.Empty<Guid>()));
    }

    [Fact]
    public void M201_Integration_Should_Have_Persistence_Ui_Security_And_No_Delete_Path()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Configurations", "M200-Business", "MachineConfiguration.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Repositories", "M200-Business", "MachineReadRepository.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Migrations", "20260425212000_M201_AddAssetTypeAndEnergyReference.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M201-Assets", "Machines", "Machines.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M201-Assets", "Machines", "MachineDetail.razor")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("AssetType", combinedContent, StringComparison.Ordinal);
        Assert.Contains("EnergyObjectId", combinedContent, StringComparison.Ordinal);
        Assert.Contains("IExecutionPipeline", combinedContent, StringComparison.Ordinal);
        Assert.Contains("MachineComponentsSection", combinedContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Delete" + "Machine", combinedContent, StringComparison.Ordinal);
        Assert.DoesNotContain(">L" + "öschen<", combinedContent, StringComparison.Ordinal);
    }

    private static Machine CreateMachine(ChangeContext changeContext, Guid energyObjectId)
    {
        return Machine.Create(
            MachineName.Create("Line 1"),
            MachineInventoryNumber.Create("M-001"),
            MachineLocation.Create("Hall A"),
            MachineStatus.Create("Active"),
            MachineAssetType.Create("Production line"),
            MachineManufacturer.Create("SWFC"),
            MachineModel.Create("L1"),
            MachineSerialNumber.Create("SN-001"),
            MachineDescription.Create("Line"),
            null,
            Guid.NewGuid(),
            energyObjectId,
            changeContext);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
