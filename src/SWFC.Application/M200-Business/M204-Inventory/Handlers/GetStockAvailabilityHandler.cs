using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class GetStockAvailabilityHandler
    : IUseCaseHandler<GetStockAvailabilityQuery, StockAvailabilityDto>
{
    private readonly IStockReservationWriteRepository _repository;
    private readonly IInventoryAvailabilityCalculator _calculator;

    public GetStockAvailabilityHandler(
        IStockReservationWriteRepository repository,
        IInventoryAvailabilityCalculator calculator)
    {
        _repository = repository;
        _calculator = calculator;
    }

    public async Task<Result<StockAvailabilityDto>> HandleAsync(
        GetStockAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        var stock = await _repository.GetStockByIdAsync(query.StockId, cancellationToken);

        if (stock is null)
        {
            return Result<StockAvailabilityDto>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"Stock '{query.StockId}' was not found.",
                ErrorCategory.NotFound));
        }

        var result = _calculator.Calculate(stock);

        return Result<StockAvailabilityDto>.Success(
            new StockAvailabilityDto(
                stock.Id,
                result.QuantityOnHand,
                result.ReservedQuantity,
                result.AvailableQuantity));
    }
}