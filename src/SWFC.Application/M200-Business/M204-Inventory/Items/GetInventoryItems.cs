using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record GetInventoryItemsQuery;

public sealed class GetInventoryItemsPolicy : IAuthorizationPolicy<GetInventoryItemsQuery>
{
    public AuthorizationRequirement GetRequirement(GetInventoryItemsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.read" });
    }
}

public sealed class GetInventoryItemsHandler : IUseCaseHandler<GetInventoryItemsQuery, IReadOnlyList<InventoryItemListItem>>
{
    private readonly IInventoryItemReadRepository _inventoryItemReadRepository;

    public GetInventoryItemsHandler(IInventoryItemReadRepository inventoryItemReadRepository)
    {
        _inventoryItemReadRepository = inventoryItemReadRepository;
    }

    public async Task<Result<IReadOnlyList<InventoryItemListItem>>> HandleAsync(
        GetInventoryItemsQuery command,
        CancellationToken cancellationToken = default)
    {
        var inventoryItems = await _inventoryItemReadRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<InventoryItemListItem>>.Success(inventoryItems);
    }
}

