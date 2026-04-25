using SWFC.Application.M200_Business.M204_Inventory.Reservations;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed record GetStockAvailabilityQuery(Guid StockId);

public sealed class GetStockAvailabilityValidator : ICommandValidator<GetStockAvailabilityQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetStockAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (query.StockId == Guid.Empty)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Stock id is required."));
        }

        return Task.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors.ToArray()));
    }
}

public sealed class GetStockAvailabilityPolicy : IAuthorizationPolicy<GetStockAvailabilityQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockAvailabilityQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stock.read" });
    }
}

public sealed class GetStockAvailabilityHandler
    : IUseCaseHandler<GetStockAvailabilityQuery, StockAvailabilityResult>
{
    private readonly IStockReadRepository _stockReadRepository;
    private readonly IStockReservationWriteRepository _stockReservationWriteRepository;
    private readonly IInventoryAvailabilityCalculator _inventoryAvailabilityCalculator;

    public GetStockAvailabilityHandler(
        IStockReadRepository stockReadRepository,
        IStockReservationWriteRepository stockReservationWriteRepository,
        IInventoryAvailabilityCalculator inventoryAvailabilityCalculator)
    {
        _stockReadRepository = stockReadRepository;
        _stockReservationWriteRepository = stockReservationWriteRepository;
        _inventoryAvailabilityCalculator = inventoryAvailabilityCalculator;
    }

    public async Task<Result<StockAvailabilityResult>> HandleAsync(
        GetStockAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        var stock = await _stockReadRepository.GetByIdAsync(query.StockId, cancellationToken);

        if (stock is null)
        {
            return Result<StockAvailabilityResult>.Failure(
                new Error(GeneralErrorCodes.NotFound, "Stock was not found.", ErrorCategory.NotFound));
        }

        var availability = _inventoryAvailabilityCalculator.Calculate(stock);
        return Result<StockAvailabilityResult>.Success(availability);
    }
}

