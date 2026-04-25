using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed record GetStockLookupQuery(
    Guid? InventoryItemId = null,
    Guid? LocationId = null);

public sealed class GetStockLookupPolicy : IAuthorizationPolicy<GetStockLookupQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockLookupQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stock.read" });
    }
}

public sealed class GetStockLookupHandler : IUseCaseHandler<GetStockLookupQuery, IReadOnlyList<StockLookupItem>>
{
    private readonly Reservations.IStockReservationReadRepository _stockReservationReadRepository;

    public GetStockLookupHandler(Reservations.IStockReservationReadRepository stockReservationReadRepository)
    {
        _stockReservationReadRepository = stockReservationReadRepository;
    }

    public async Task<Result<IReadOnlyList<StockLookupItem>>> HandleAsync(
        GetStockLookupQuery command,
        CancellationToken cancellationToken = default)
    {
        var items = await _stockReservationReadRepository.GetStockLookupAsync(
            command.InventoryItemId,
            command.LocationId,
            cancellationToken);

        return Result<IReadOnlyList<StockLookupItem>>.Success(items);
    }
}

