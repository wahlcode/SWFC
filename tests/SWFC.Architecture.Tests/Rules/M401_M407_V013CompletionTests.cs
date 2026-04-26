using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SWFC.Architecture.Tests.Support;
using SWFC.Infrastructure.M400_Integration.M401_ImportExport;
using SWFC.Infrastructure.M400_Integration.M402_API;
using SWFC.Infrastructure.M400_Integration.M403_ERPIntegration;
using SWFC.Infrastructure.M400_Integration.M404_IoTMaschinen;
using SWFC.Infrastructure.M400_Integration.M405_MessagingEvents;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;
using SWFC.Infrastructure.M400_Integration.M407_DMSFileIntegration;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M401_M407_V013CompletionTests
{
    [Fact]
    public void V013_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.13.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M401", "M402", "M403", "M404", "M405", "M406", "M407" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M401_To_M407_WorkItems_Should_Be_Done_For_V013()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var integrationModules = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Where(module => (module.GetProperty("Code").GetString() ?? string.Empty) is
                "M401" or "M402" or "M403" or "M404" or "M405" or "M406" or "M407")
            .ToArray();

        Assert.Equal(7, integrationModules.Length);

        foreach (var module in integrationModules)
        {
            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public async Task M401_Should_Map_Csv_Rows_And_Reject_Invalid_Technical_Values()
    {
        var adapter = new StructuredDataExchangeAdapter();
        var target = new CapturingIntegrationTarget();
        var request = new StructuredImportRequest(
            "stock.csv",
            StructuredDataFormat.Csv,
            Encoding.UTF8.GetBytes("ItemNo,Quantity,PostedAt\nA-100,12,2026-04-26\nA-200,not-a-number,2026-04-26\n"),
            "M204",
            "CreateStockMovement",
            [
                new("ItemNo", "InventoryItemNumber"),
                new("Quantity", "QuantityDelta", IntegrationFieldKind.Integer),
                new("PostedAt", "OccurredAtUtc", IntegrationFieldKind.DateTime)
            ],
            ["ItemNo", "Quantity"]);

        var result = await adapter.ImportAsync(request, target);

        Assert.Equal(2, result.TotalRows);
        Assert.Equal(1, result.AcceptedRows);
        Assert.Equal(1, result.RejectedRows);
        Assert.Single(target.Records);
        Assert.Equal("M204", target.Records[0].TargetModule);
        Assert.Equal(12, target.Records[0].Fields["QuantityDelta"]);
        Assert.Contains(result.Rows, row => !row.Accepted && row.Messages.Any(message => message.Contains("Quantity", StringComparison.Ordinal)));
    }

    [Fact]
    public void M401_Should_Export_Csv_And_Excel_Without_Target_Module_Logic()
    {
        var adapter = new StructuredDataExchangeAdapter();
        var records = new[]
        {
            new Dictionary<string, object?> { ["Number"] = "A-100", ["Quantity"] = 12 }
        };

        var csv = adapter.Export(new StructuredExportRequest("items", StructuredDataFormat.Csv, ["Number", "Quantity"], records));
        var excel = adapter.Export(new StructuredExportRequest("items", StructuredDataFormat.Excel, ["Number", "Quantity"], records));

        Assert.Equal("text/csv", csv.ContentType);
        Assert.Contains("A-100", Encoding.UTF8.GetString(csv.Content), StringComparison.Ordinal);
        Assert.Equal("application/vnd.ms-excel", excel.ContentType);
        Assert.Contains("Worksheet", Encoding.UTF8.GetString(excel.Content), StringComparison.Ordinal);
    }

    [Fact]
    public async Task M402_Should_Expose_Authenticated_Module_Api_Catalog_Without_Database_Access()
    {
        var catalog = new IntegrationApiCatalog();
        var gateway = new IntegrationApiGateway(catalog, Array.Empty<IIntegrationApiModuleAdapter>());

        var response = await gateway.ExecuteAsync(new IntegrationApiRequest(
            "GET",
            "/api/v1/integration/modules",
            IsAuthenticated: false,
            Query: new Dictionary<string, string?>(),
            Body: null));

        Assert.Equal(401, response.StatusCode);
        Assert.Contains(catalog.GetEndpoints(), endpoint => endpoint.Route == "/api/v1/assets/machines" && endpoint.TargetUseCase == "GetVisibleMachinesQuery");
        Assert.Contains(catalog.GetEndpoints(), endpoint => endpoint.Method == "PUT" && endpoint.TargetUseCase == "UpdateEnergyReadingCommand");
        Assert.All(catalog.GetEndpoints(), endpoint => Assert.True(endpoint.RequiresAuthentication));

        var moduleSource = File.ReadAllText(RepositoryRoot.Combine(
            "src", "SWFC.Infrastructure", "M400-Integration", "M402-API", "IntegrationApiGateway.cs"));
        Assert.DoesNotContain("DbContext", moduleSource, StringComparison.Ordinal);
        Assert.DoesNotContain("UseNpgsql", moduleSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task M403_Should_Map_Erp_Master_Data_And_Transfer_Purchasing_References()
    {
        var transport = new CapturingErpTransport();
        var adapter = new ErpIntegrationAdapter(transport);
        var supplier = adapter.MapInboundMasterData(new ErpObjectReference(
            ErpObjectKind.Supplier,
            "SAP",
            "SUP-100",
            "MRO Supplier",
            new Dictionary<string, string?> { ["Country"] = "DE" }));
        var order = await adapter.TransferPurchaseOrderAsync(
            "SAP",
            new ErpPurchaseOrderTransfer(Guid.NewGuid(), "PO-100", Guid.NewGuid(), null, "M104:po/PO-100.pdf"));

        Assert.Equal(ErpTransferDirection.InboundFromErp, supplier.Direction);
        Assert.Equal("ERP", supplier.Payload["CommercialAuthority"]);
        Assert.Equal("M206", order.Payload["SourceModule"]);
        Assert.Single(transport.Messages);
        Assert.Equal(ErpObjectKind.PurchaseOrder, transport.Messages[0].ObjectKind);
    }

    [Fact]
    public async Task M404_Should_Forward_Raw_Machine_Telemetry_Without_Interpretation()
    {
        var sink = new CapturingTelemetrySink();
        var adapter = new MachineTelemetryAdapter(sink);
        var packet = new MachineTelemetryPacket(
            "PCS",
            "M-100",
            DateTime.UtcNow,
            [new MachineTelemetryPoint("energy.active", MachineTelemetryKind.Measurement, "123.45", "kWh", "M205")],
            new Dictionary<string, string?> { ["line"] = "A" });

        var envelope = await adapter.TransferAsync(packet);

        Assert.Single(sink.Envelopes);
        Assert.Equal("123.45", envelope.Points[0].RawValue);
        Assert.Contains("M205", envelope.SupportedConsumerModules);
        Assert.Contains("M212", envelope.SupportedConsumerModules);
        Assert.Contains("M303", envelope.SupportedConsumerModules);
    }

    [Fact]
    public async Task M405_Should_Distribute_Events_To_Decoupled_Handlers()
    {
        var handler = new CapturingEventHandler("MachineFaultReported", "notification-bridge");
        var dispatcher = new IntegrationEventDispatcher([handler]);
        var envelope = new IntegrationEventEnvelope(
            "MachineFaultReported",
            "M404",
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow,
            new Dictionary<string, object?> { ["Machine"] = "M-100" });

        var deliveries = await dispatcher.PublishAsync(envelope);

        Assert.Single(deliveries);
        Assert.True(deliveries[0].Delivered);
        Assert.Single(handler.Envelopes);
        Assert.Equal("M404", handler.Envelopes[0].SourceModule);
    }

    [Fact]
    public void M406_Should_Register_Oidc_Provider_And_Map_Claims_Without_Access_Decisions()
    {
        var options = new IdentityIntegrationOptions
        {
            Oidc = new OidcProviderOptions
            {
                Enabled = true,
                DisplayName = "Keycloak",
                Authority = "https://identity.example.test/realms/swfc",
                ClientId = "swfc",
                Scopes = ["openid", "profile", "email", "groups"]
            }
        };
        var descriptor = new OidcProviderRegistry().BuildDescriptor(options.Oidc);
        var resolver = new OidcExternalIdentityResolver(Options.Create(options));
        var identity = resolver.Resolve(new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", "external-123"),
            new Claim("email", "user@example.test"),
            new Claim("name", "External User"),
            new Claim("amr", "pwd")
        ])));

        Assert.Equal("https://identity.example.test/realms/swfc", descriptor.Authority);
        Assert.Contains("groups", descriptor.Scopes);
        Assert.Equal("external-123", identity.Subject);
        Assert.Equal("user@example.test", identity.IdentityKey);
        Assert.DoesNotContain(descriptor.Scopes, scope => scope.Contains("permission", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task M407_Should_Create_Dms_Reference_And_Hand_It_To_Document_Module()
    {
        var sink = new CapturingDocumentReferenceSink();
        var adapter = new DmsFileReferenceAdapter(sink);
        var reference = await adapter.LinkAsync(new DmsExternalDocumentRequest(
            "SharePoint",
            "DOC-100",
            new Uri("https://dms.example.test/docs/DOC-100"),
            "manual.pdf",
            "application/pdf",
            1024,
            "M104",
            "Document",
            "document-100"));

        Assert.Single(sink.References);
        Assert.Equal("SharePoint", reference.DmsSystem);
        Assert.Equal("M104:Document:document-100:DOC-100", reference.SwfcReferenceKey);
    }

    [Fact]
    public void M400_Integration_Modules_Should_Not_Contain_Fachlogik_Or_Todo_Placeholders()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Infrastructure", "M400-Integration"),
                "*.cs",
                includeGeneratedFiles: false)
            .ToArray();

        Assert.NotEmpty(files);

        var combined = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        Assert.DoesNotContain("TODO", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Placeholder", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveChanges", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplyMovement", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateMaintenanceOrder", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("PermissionDecision", combined, StringComparison.Ordinal);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();

    private sealed class CapturingIntegrationTarget : IIntegrationModuleTarget
    {
        public List<IntegrationMappedRecord> Records { get; } = [];

        public Task<IntegrationTargetResult> SubmitAsync(
            IntegrationMappedRecord record,
            CancellationToken cancellationToken = default)
        {
            Records.Add(record);
            return Task.FromResult(IntegrationTargetResult.Success(record.SourceRowNumber.ToString()));
        }
    }

    private sealed class CapturingErpTransport : IErpTransportAdapter
    {
        public List<ErpTransferMessage> Messages { get; } = [];

        public Task SubmitAsync(
            ErpTransferMessage message,
            CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingTelemetrySink : IMachineTelemetrySink
    {
        public List<MachineTelemetryEnvelope> Envelopes { get; } = [];

        public Task ForwardAsync(
            MachineTelemetryEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            Envelopes.Add(envelope);
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingEventHandler : IIntegrationEventHandler
    {
        public CapturingEventHandler(string eventName, string handlerName)
        {
            EventName = eventName;
            HandlerName = handlerName;
        }

        public string EventName { get; }

        public string HandlerName { get; }

        public List<IntegrationEventEnvelope> Envelopes { get; } = [];

        public Task<IntegrationEventDelivery> HandleAsync(
            IntegrationEventEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            Envelopes.Add(envelope);
            return Task.FromResult(new IntegrationEventDelivery(EventName, HandlerName, true, "Delivered."));
        }
    }

    private sealed class CapturingDocumentReferenceSink : IDocumentReferenceSink
    {
        public List<ExternalDocumentReference> References { get; } = [];

        public Task RegisterReferenceAsync(
            ExternalDocumentReference reference,
            CancellationToken cancellationToken = default)
        {
            References.Add(reference);
            return Task.CompletedTask;
        }
    }
}
