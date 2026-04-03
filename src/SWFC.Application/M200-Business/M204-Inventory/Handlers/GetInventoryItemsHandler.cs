using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

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