namespace SWFC.Infrastructure.M400_Integration.M402_API;

public sealed record IntegrationApiEndpointDescriptor(
    string ModuleCode,
    string Method,
    string Route,
    bool RequiresAuthentication,
    string AdapterName,
    string TargetUseCase);

public sealed record IntegrationApiRequest(
    string Method,
    string Route,
    bool IsAuthenticated,
    IReadOnlyDictionary<string, string?> Query,
    object? Body);

public sealed record IntegrationApiResponse(
    int StatusCode,
    object? Payload,
    string? Error)
{
    public static IntegrationApiResponse Ok(object? payload)
        => new(200, payload, null);

    public static IntegrationApiResponse Unauthorized()
        => new(401, null, "Authentication is required.");

    public static IntegrationApiResponse NotFound(string route)
        => new(404, null, $"No integration API adapter is registered for '{route}'.");

    public static IntegrationApiResponse BadRequest(string message)
        => new(400, null, message);
}

public interface IIntegrationApiModuleAdapter
{
    bool CanHandle(IntegrationApiRequest request);

    Task<IntegrationApiResponse> ExecuteAsync(
        IntegrationApiRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class IntegrationApiCatalog
{
    private static readonly IReadOnlyList<IntegrationApiEndpointDescriptor> Endpoints =
    [
        new("M401", "POST", "/api/v1/imports", true, "StructuredDataExchangeAdapter", "technical import handoff"),
        new("M401", "GET", "/api/v1/exports", true, "StructuredDataExchangeAdapter", "technical export handoff"),
        new("M402", "GET", "/api/v1/integration/modules", true, "IntegrationApiCatalog", "integration endpoint catalog"),
        new("M201", "GET", "/api/v1/assets/machines", true, "M402 endpoint wrapper", "GetVisibleMachinesQuery"),
        new("M204", "GET", "/api/v1/inventory/items", true, "M402 endpoint wrapper", "GetInventoryItemsQuery"),
        new("M205", "POST", "/api/v1/energy/readings", true, "M402 endpoint wrapper", "CreateEnergyReadingCommand"),
        new("M205", "PUT", "/api/v1/energy/readings/{id}", true, "M402 endpoint wrapper", "UpdateEnergyReadingCommand"),
        new("M403", "POST", "/api/v1/erp/purchase-orders", true, "ErpIntegrationAdapter", "M206 purchase order transfer"),
        new("M404", "POST", "/api/v1/iot/telemetry", true, "MachineTelemetryAdapter", "machine telemetry handoff"),
        new("M405", "POST", "/api/v1/events", true, "IntegrationEventDispatcher", "event transport"),
        new("M407", "POST", "/api/v1/dms/references", true, "DmsFileReferenceAdapter", "M104 document reference handoff")
    ];

    public IReadOnlyList<IntegrationApiEndpointDescriptor> GetEndpoints()
    {
        return Endpoints;
    }
}

public sealed class IntegrationApiGateway
{
    private readonly IntegrationApiCatalog _catalog;
    private readonly IEnumerable<IIntegrationApiModuleAdapter> _adapters;

    public IntegrationApiGateway(
        IntegrationApiCatalog catalog,
        IEnumerable<IIntegrationApiModuleAdapter> adapters)
    {
        _catalog = catalog;
        _adapters = adapters;
    }

    public async Task<IntegrationApiResponse> ExecuteAsync(
        IntegrationApiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var descriptor = _catalog.GetEndpoints().FirstOrDefault(endpoint =>
            string.Equals(endpoint.Method, request.Method, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(endpoint.Route, request.Route, StringComparison.OrdinalIgnoreCase));

        if (descriptor is null)
        {
            return IntegrationApiResponse.NotFound(request.Route);
        }

        if (descriptor.RequiresAuthentication && !request.IsAuthenticated)
        {
            return IntegrationApiResponse.Unauthorized();
        }

        var adapter = _adapters.FirstOrDefault(x => x.CanHandle(request));

        return adapter is null
            ? IntegrationApiResponse.NotFound(request.Route)
            : await adapter.ExecuteAsync(request, cancellationToken);
    }
}
