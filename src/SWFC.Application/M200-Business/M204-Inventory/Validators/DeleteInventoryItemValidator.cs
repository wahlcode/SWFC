using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class DeleteInventoryItemValidator : ICommandValidator<DeleteInventoryItemCommand>
{
    public Task<ValidationResult> ValidateAsync(
        DeleteInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Inventory item id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}