using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class ReleaseStockReservationValidator : ICommandValidator<ReleaseStockReservationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        ReleaseStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.ReservationId == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Reservation id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}