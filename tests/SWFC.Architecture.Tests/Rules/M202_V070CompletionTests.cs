using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M202_V070CompletionTests
{
    [Fact]
    public void V070_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.7.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M202", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M201", "M204", "M102", "M805" },
            required => Assert.Contains(required, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M202_WorkItems_Should_Be_Done_For_V070()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m202 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M202");

        Assert.Equal("Full Complete", m202.GetProperty("Status").GetString());
        Assert.Equal(100, m202.GetProperty("ProgressPercent").GetInt32());
        Assert.All(
            m202.GetProperty("WorkItems").EnumerateArray(),
            item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M202_Domain_Should_Plan_Orders_Track_Status_And_Reference_Material_Only()
    {
        var changeContext = ChangeContext.Create("tester", "v0.7 verification");
        var targetId = Guid.NewGuid();
        var plan = MaintenancePlan.Create(
            new MaintenancePlanName("Monthly press check"),
            new MaintenancePlanDescription("Scheduled recurring inspection"),
            MaintenanceTargetType.Machine,
            targetId,
            1,
            MaintenancePlanIntervalUnit.Months,
            new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            changeContext);

        var itemId = Guid.NewGuid();
        var order = MaintenanceOrder.Create(
            new MaintenanceOrderNumber("MO-001"),
            new MaintenanceOrderTitle("Press maintenance"),
            new MaintenanceOrderDescription("Check lubrication and wear."),
            MaintenanceOrderType.Planned,
            MaintenanceOrderPriority.High,
            MaintenanceTargetType.Machine,
            targetId,
            plan.Id,
            new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            changeContext);

        order.AddMaterial(itemId, 2, changeContext);
        order.Update(
            new MaintenanceOrderTitle("Press maintenance done"),
            new MaintenanceOrderDescription("Finished with referenced spare parts."),
            MaintenanceOrderType.Planned,
            MaintenanceOrderPriority.Critical,
            MaintenanceOrderStatus.Completed,
            MaintenanceTargetType.Machine,
            targetId,
            plan.Id,
            order.PlannedStartUtc,
            order.PlannedEndUtc,
            order.DueAtUtc,
            new[] { (itemId, 3) },
            ChangeContext.Create("tester", "complete order"));

        Assert.Equal(MaintenanceOrderStatus.Completed, order.Status);
        Assert.Equal(MaintenanceOrderPriority.Critical, order.Priority);
        Assert.Equal(plan.Id, order.MaintenancePlanId);
        Assert.NotNull(order.StartedAtUtc);
        Assert.NotNull(order.CompletedAtUtc);
        Assert.Equal(itemId, order.Materials.Single().ItemId);
        Assert.Equal(3, order.Materials.Single().Quantity);
    }

    [Fact]
    public void M202_Integration_Should_Have_Persistence_Ui_Security_And_No_Stock_Mutation()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M202-Maintenance", "MaintenanceOrders", "MaintenanceOrder.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M202-Maintenance", "MaintenanceOrders", "UpdateMaintenanceOrder.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Configurations", "M200-Business", "MaintenanceOrderConfiguration.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Migrations", "20260425233000_M202_AddMaintenanceOrderOperationalFields.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Repositories", "M200-Business", "MaintenanceOrderWriteRepository.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M202-Maintenance", "Maintenance.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M202-Maintenance", "Components", "MaintenanceOrderForm.razor")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("MaintenanceOrderPriority", combinedContent, StringComparison.Ordinal);
        Assert.Contains("MaintenancePlanId", combinedContent, StringComparison.Ordinal);
        Assert.Contains("DueAtUtc", combinedContent, StringComparison.Ordinal);
        Assert.Contains("IExecutionPipeline", combinedContent, StringComparison.Ordinal);
        Assert.DoesNotContain("IStockWriteRepository", combinedContent, StringComparison.Ordinal);
        Assert.DoesNotContain("StockMovement", combinedContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Delete" + "Maintenance", combinedContent, StringComparison.Ordinal);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
