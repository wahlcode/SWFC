using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M204_V060CompletionTests
{
    [Fact]
    public void V060_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.6.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M204", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M201", "M102", "M805", "M806" },
            required => Assert.Contains(required, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M204_WorkItems_Should_Be_Done_For_V060()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m204 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M204");

        Assert.Equal("Full Complete", m204.GetProperty("Status").GetString());
        Assert.Equal(100, m204.GetProperty("ProgressPercent").GetInt32());
        Assert.All(
            m204.GetProperty("WorkItems").EnumerateArray(),
            item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M204_Domain_Should_Change_Stock_Only_By_Movements_And_Protect_Availability()
    {
        var changeContext = ChangeContext.Create("tester", "v0.6 verification");
        var item = CreateInventoryItem(changeContext);
        var source = Stock.Create(item.Id, Guid.NewGuid(), "A-01", 0, changeContext);
        var target = Stock.Create(item.Id, Guid.NewGuid(), "B-01", 0, changeContext);

        Assert.Equal(12.50m, item.StandardUnitPrice.Value);
        Assert.Equal("EUR", item.Currency.Value);

        source.ApplyMovement(StockMovement.Create(source.Id, StockMovementType.GoodsReceipt, 10, changeContext), changeContext);
        Assert.Equal(10, source.QuantityOnHand);

        var reservation = StockReservation.Create(
            source.Id,
            4,
            "Project demand",
            InventoryTargetType.Project,
            Guid.NewGuid().ToString(),
            changeContext);
        source.AddReservation(reservation, changeContext);

        Assert.Equal(4, source.GetReservedQuantity());
        Assert.Equal(6, source.GetAvailableQuantity());
        Assert.Throws<ValidationException>(() => source.AddReservation(
            StockReservation.Create(source.Id, 7, "Too much", null, null, changeContext),
            changeContext));

        source.ApplyMovement(StockMovement.Create(source.Id, StockMovementType.Transfer, -3, changeContext), changeContext);
        target.ApplyMovement(StockMovement.Create(target.Id, StockMovementType.Transfer, 3, changeContext), changeContext);

        Assert.Equal(7, source.QuantityOnHand);
        Assert.Equal(3, target.QuantityOnHand);
        Assert.Throws<ValidationException>(() => source.ApplyMovement(
            StockMovement.Create(source.Id, StockMovementType.GoodsIssue, -20, changeContext),
            changeContext));
    }

    [Fact]
    public async Task M204_Validators_Should_Require_Reason_Target_Pairs_And_Real_Transfer_Target()
    {
        var validator = new CreateStockMovementValidator();

        var receiptResult = await validator.ValidateAsync(new CreateStockMovementCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            StockMovementType.GoodsReceipt,
            1,
            InventoryTargetType.Asset,
            null,
            "reason"));

        Assert.False(receiptResult.IsValid);

        var transferResult = await validator.ValidateAsync(new CreateStockMovementCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A",
            StockMovementType.Transfer,
            -1,
            InventoryTargetType.Asset,
            Guid.NewGuid().ToString(),
            "reason",
            Guid.NewGuid(),
            "B"));

        Assert.True(transferResult.IsValid);
    }

    [Fact]
    public void M204_Integration_Should_Have_Persistence_Ui_Audit_And_No_Direct_Stock_Setter()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M204-Inventory", "Stock", "CreateStockMovement.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M204-Inventory", "Items", "InventoryItemStandardUnitPrice.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Migrations", "20260425224500_M204_AddInventoryItemValuation.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Repositories", "M200-Business", "StockMovementWriteRepository.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "DependencyInjection", "ServiceCollectionExtensions.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M204-Inventory", "Stock", "StockMovementDetail.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("IAuditService", combinedContent, StringComparison.Ordinal);
        Assert.Contains("StockMovementType.Transfer", combinedContent, StringComparison.Ordinal);
        Assert.Contains("TransferLocationId", combinedContent, StringComparison.Ordinal);
        Assert.Contains("StandardUnitPrice", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AddScoped<IStockMovementWriteRepository", combinedContent, StringComparison.Ordinal);
        Assert.Contains("/business/inventory", combinedContent, StringComparison.Ordinal);

        var stockDomain = File.ReadAllText(RepositoryRoot.Combine(
            "src", "SWFC.Domain", "M200-Business", "M204-Inventory", "Stock", "Stock.cs"));
        Assert.DoesNotContain("ApplyInventoryCorrection", stockDomain, StringComparison.Ordinal);
        Assert.DoesNotContain("Relocate(", stockDomain, StringComparison.Ordinal);
    }

    private static InventoryItem CreateInventoryItem(ChangeContext changeContext)
    {
        return InventoryItem.Create(
            InventoryItemArticleNumber.Create("ART-001"),
            InventoryItemName.Create("Bearing"),
            InventoryItemDescription.Create("Bearing unit"),
            InventoryItemUnit.Create("pcs"),
            InventoryItemBarcode.CreateOptional("4006381333931"),
            InventoryItemManufacturer.CreateOptional("SWFC"),
            InventoryItemManufacturerPartNumber.CreateOptional("BR-1"),
            InventoryItemStandardUnitPrice.Create(12.50m),
            InventoryItemCurrency.Create("EUR"),
            changeContext);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
