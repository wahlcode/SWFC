using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

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
        var reservations = await _stockReservationReadRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<StockReservationListItem>>.Success(reservations);
    }
}