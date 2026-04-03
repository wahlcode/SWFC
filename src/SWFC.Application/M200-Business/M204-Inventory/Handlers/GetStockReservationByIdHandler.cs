using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class GetStockReservationByIdHandler : IUseCaseHandler<GetStockReservationByIdQuery, StockReservationDetailsDto>
{
    private readonly IStockReservationReadRepository _stockReservationReadRepository;

    public GetStockReservationByIdHandler(IStockReservationReadRepository stockReservationReadRepository)
    {
        _stockReservationReadRepository = stockReservationReadRepository;
    }

    public async Task<Result<StockReservationDetailsDto>> HandleAsync(
        GetStockReservationByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _stockReservationReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (reservation is null)
        {
            return Result<StockReservationDetailsDto>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"StockReservation '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<StockReservationDetailsDto>.Success(reservation);
    }
}