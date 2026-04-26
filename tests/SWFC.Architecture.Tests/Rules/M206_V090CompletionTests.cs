using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M206_V090CompletionTests
{
    [Fact]
    public void V090_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.9.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M206", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M204", "M202", "M209", "M403", "M805", "M806" },
            required => Assert.Contains(required, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M206_WorkItems_Should_Be_Done_For_V090()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m206 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M206");

        Assert.Equal("Full Complete", m206.GetProperty("Status").GetString());
        Assert.Equal(100, m206.GetProperty("ProgressPercent").GetInt32());
        Assert.All(
            m206.GetProperty("WorkItems").EnumerateArray(),
            item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M206_Domain_Should_Model_Operational_Purchasing_Without_Financial_Processing()
    {
        var supplier = new Supplier(Guid.NewGuid(), "MRO Supplier", "SUP-001", DateTime.UtcNow);
        var requirement = new PurchaseRequirement(
            Guid.NewGuid(),
            "Bearing",
            2,
            "pcs",
            PurchaseRequirementSourceType.Maintenance,
            Guid.NewGuid(),
            DateTime.UtcNow);
        var order = new PurchaseOrder(
            Guid.NewGuid(),
            "PO-1001",
            supplier.Id,
            DateTime.UtcNow,
            "SAP-MM-42",
            "M104:purchase-order/PO-1001.pdf");
        var rfq = new RequestForQuotation(
            Guid.NewGuid(),
            requirement.Id,
            supplier.Id,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7),
            "M104:offer/RFQ-1001.pdf");
        var receipt = new GoodsReceipt(
            Guid.NewGuid(),
            order.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "A-01",
            2,
            "pcs",
            DateTime.UtcNow,
            "M104:delivery-note/DN-1001.pdf");

        Assert.True(supplier.IsActive);
        Assert.Equal(PurchaseRequirementSourceType.Maintenance, requirement.SourceType);
        Assert.Equal("SAP-MM-42", order.ErpReference);
        Assert.Equal("M104:purchase-order/PO-1001.pdf", order.OrderDocumentReference);
        Assert.Equal("M104:offer/RFQ-1001.pdf", rfq.OfferDocumentReference);
        Assert.Equal("M104:delivery-note/DN-1001.pdf", receipt.DeliveryDocumentReference);
        Assert.Equal(GoodsReceiptInventoryBookingStatus.Requested, receipt.InventoryBookingStatus);

        var m206Domain = string.Join(Environment.NewLine, RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Domain", "M200-Business", "M206-Purchasing"),
                "*.cs",
                includeGeneratedFiles: false)
            .Select(File.ReadAllText));

        Assert.DoesNotContain("Invoice", m206Domain, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Payment", m206Domain, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tax", m206Domain, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task M206_PurchaseProposals_Should_Derive_Manual_And_Automatic_Suggestions_From_Open_Requirements()
    {
        var manual = new PurchaseRequirementDto(
            Guid.NewGuid(),
            "Manual demand",
            1,
            "pcs",
            PurchaseRequirementSourceType.Manual,
            null,
            PurchaseRequirementStatus.Open,
            DateTime.UtcNow,
            null);
        var minimumStock = new PurchaseRequirementDto(
            Guid.NewGuid(),
            "Minimum stock demand",
            5,
            "pcs",
            PurchaseRequirementSourceType.InventoryMinimumStock,
            Guid.NewGuid(),
            PurchaseRequirementStatus.Open,
            DateTime.UtcNow,
            null);
        var closed = new PurchaseRequirementDto(
            Guid.NewGuid(),
            "Closed demand",
            1,
            "pcs",
            PurchaseRequirementSourceType.Project,
            Guid.NewGuid(),
            PurchaseRequirementStatus.Closed,
            DateTime.UtcNow,
            null);

        var useCase = new GetPurchaseProposals(new StubPurchaseRequirementReadRepository([manual, minimumStock, closed]));

        var proposals = await useCase.ExecuteAsync();

        Assert.Equal(2, proposals.Count);
        Assert.Contains(proposals, x => x.PurchaseRequirementId == manual.Id && !x.IsAutomatic);
        Assert.Contains(proposals, x => x.PurchaseRequirementId == minimumStock.Id && x.IsAutomatic);
        Assert.DoesNotContain(proposals, x => x.PurchaseRequirementId == closed.Id);
    }

    [Fact]
    public void M206_Integration_Should_Have_Persistence_Ui_M204_Booking_And_No_Delete_Use_Cases()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M206-Purchasing", "PurchaseRequirements", "CreatePurchaseRequirement.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M206-Purchasing", "PurchaseRequirements", "GetPurchaseProposals.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M206-Purchasing", "PurchaseOrders", "CreatePurchaseOrder.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M206-Purchasing", "GoodsReceipts", "CreateGoodsReceipt.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M206-Purchasing", "RequestForQuotations", "CreateRequestForQuotation.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Migrations", "20260426093000_M206_AddOperationalReferences.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "DependencyInjection", "ServiceCollectionExtensions.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M206-Purchasing", "Purchasing.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("PurchaseRequirementSourceType.InventoryMinimumStock", combinedContent, StringComparison.Ordinal);
        Assert.Contains("IExecutionPipeline<CreateStockMovementCommand, Guid>", combinedContent, StringComparison.Ordinal);
        Assert.Contains("StockMovementType.GoodsReceipt", combinedContent, StringComparison.Ordinal);
        Assert.Contains("InventoryTargetType.GoodsReceipt", combinedContent, StringComparison.Ordinal);
        Assert.Contains("ErpReference", combinedContent, StringComparison.Ordinal);
        Assert.Contains("OrderDocumentReference", combinedContent, StringComparison.Ordinal);
        Assert.Contains("DeliveryDocumentReference", combinedContent, StringComparison.Ordinal);
        Assert.Contains("OfferDocumentReference", combinedContent, StringComparison.Ordinal);
        Assert.Contains("/m200/purchasing", combinedContent, StringComparison.Ordinal);

        var deleteUseCases = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M200-Business", "M206-Purchasing"),
                "Delete*.cs",
                includeGeneratedFiles: false)
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(deleteUseCases.Length == 0, $"M206 must not delete purchasing records: {string.Join(", ", deleteUseCases)}");
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();

    private sealed class StubPurchaseRequirementReadRepository : IPurchaseRequirementReadRepository
    {
        private readonly IReadOnlyList<PurchaseRequirementDto> _requirements;

        public StubPurchaseRequirementReadRepository(IReadOnlyList<PurchaseRequirementDto> requirements)
        {
            _requirements = requirements;
        }

        public Task<IReadOnlyList<PurchaseRequirementDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_requirements);
        }
    }
}
