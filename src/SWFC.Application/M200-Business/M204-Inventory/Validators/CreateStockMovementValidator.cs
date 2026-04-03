using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class CreateStockMovementValidator : ICommandValidator<CreateStockMovementCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateStockMovementCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.StockId == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Stock id is required.");
        }

        if (!Enum.IsDefined(command.MovementType))
        {
            result.Add(ErrorCodes.Validation.Invalid, "Movement type is invalid.");
        }

        if (command.QuantityDelta == 0)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Quantity delta must not be zero.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}