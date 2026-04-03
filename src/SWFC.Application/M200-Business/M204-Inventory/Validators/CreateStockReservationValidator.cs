using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class CreateStockReservationValidator : ICommandValidator<CreateStockReservationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.StockId == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Stock id is required.");
        }

        if (command.Quantity <= 0)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Reservation quantity must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}