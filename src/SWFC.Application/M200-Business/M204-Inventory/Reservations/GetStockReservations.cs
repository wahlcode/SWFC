using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Reservations;

public sealed record GetStockReservationsQuery(
    Guid? StockId = null,
    Guid? InventoryItemId = null,
    Guid? LocationId = null);

public sealed class GetStockReservationsPolicy : IAuthorizationPolicy<GetStockReservationsQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockReservationsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockreservation.read" });
    }
}

public sealed class GetStockReservationsHandler : IUseCaseHandler<GetStockReservationsQuery, IReadOnlyList<StockReservationListItem>>
{
    private readonly IStockReservationReadRepository _stockReservationReadRepository;

    public GetStockReservationsHandler(IStockReservationReadRepository stockReservationReadRepository)
    {
        _stockReservationReadRepository = stockReservationReadRepository;
    }

    public async Task<Result<IReadOnlyList<StockReservationListItem>>> HandleAsync(
        GetStockReservationsQuery command,
        CancellationToken cancellationToken = default)
    {
        var reservations = await _stockReservationReadRepository.GetAllAsync(
            command.StockId,
            command.InventoryItemId,
            command.LocationId,
            cancellationToken);

        return Result<IReadOnlyList<StockReservationListItem>>.Success(reservations);
    }
}

