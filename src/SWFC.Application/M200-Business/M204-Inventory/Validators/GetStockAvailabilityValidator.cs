using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Queries;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class GetStockAvailabilityValidator : ICommandValidator<GetStockAvailabilityQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetStockAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (query.StockId == Guid.Empty)
        {
            errors.Add(new ValidationError("StockId", "StockId is required."));
        }

        return Task.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors.ToArray()));
    }
}