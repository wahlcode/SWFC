using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record GetInventoryItemLookupQuery;

public sealed class GetInventoryItemLookupPolicy : IAuthorizationPolicy<GetInventoryItemLookupQuery>
{
    public AuthorizationRequirement GetRequirement(GetInventoryItemLookupQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.read" });
    }
}

public sealed class GetInventoryItemLookupHandler : IUseCaseHandler<GetInventoryItemLookupQuery, IReadOnlyList<InventoryItemLookupItem>>
{
    private readonly IInventoryItemReadRepository _inventoryItemReadRepository;

    public GetInventoryItemLookupHandler(IInventoryItemReadRepository inventoryItemReadRepository)
    {
        _inventoryItemReadRepository = inventoryItemReadRepository;
    }

    public async Task<Result<IReadOnlyList<InventoryItemLookupItem>>> HandleAsync(
        GetInventoryItemLookupQuery command,
        CancellationToken cancellationToken = default)
    {
        var items = await _inventoryItemReadRepository.GetLookupAsync(cancellationToken);
        return Result<IReadOnlyList<InventoryItemLookupItem>>.Success(items);
    }
}

