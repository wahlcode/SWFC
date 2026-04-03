using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class GetStockReservationByIdValidator : ICommandValidator<GetStockReservationByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetStockReservationByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Reservation id is required.");
        }

        return Task.FromResult(result);
    }
}