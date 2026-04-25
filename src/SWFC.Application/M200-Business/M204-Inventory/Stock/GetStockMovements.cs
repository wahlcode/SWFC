using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed record GetStockMovementsQuery(
    Guid? StockId = null,
    Guid? InventoryItemId = null,
    Guid? LocationId = null);

public sealed class GetStockMovementsPolicy : IAuthorizationPolicy<GetStockMovementsQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockMovementsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockmovement.read" });
    }
}

public sealed class GetStockMovementsHandler : IUseCaseHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItem>>
{
    private readonly IStockMovementReadRepository _stockMovementReadRepository;

    public GetStockMovementsHandler(IStockMovementReadRepository stockMovementReadRepository)
    {
        _stockMovementReadRepository = stockMovementReadRepository;
    }

    public async Task<Result<IReadOnlyList<StockMovementListItem>>> HandleAsync(
        GetStockMovementsQuery command,
        CancellationToken cancellationToken = default)
    {
        var movements = await _stockMovementReadRepository.GetAllAsync(
            command.StockId,
            command.InventoryItemId,
            command.LocationId,
            cancellationToken);

        return Result<IReadOnlyList<StockMovementListItem>>.Success(movements);
    }
}

