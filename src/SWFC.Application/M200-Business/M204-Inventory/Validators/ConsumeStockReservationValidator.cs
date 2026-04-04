using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Commands;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class ConsumeStockReservationValidator : ICommandValidator<ConsumeStockReservationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        ConsumeStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (command.StockReservationId == Guid.Empty)
        {
            errors.Add(new ValidationError("StockReservationId", "StockReservationId is required."));
        }

        if (command.Quantity <= 0)
        {
            errors.Add(new ValidationError("Quantity", "Quantity must be greater than zero."));
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            errors.Add(new ValidationError("Reason", "Reason is required."));
        }

        return Task.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors.ToArray()));
    }
}