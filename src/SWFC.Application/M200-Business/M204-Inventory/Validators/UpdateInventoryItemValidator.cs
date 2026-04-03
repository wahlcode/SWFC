using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class UpdateInventoryItemValidator : ICommandValidator<UpdateInventoryItemCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Inventory item id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add(ErrorCodes.Validation.Invalid, "Inventory item name is required.");
        }
        else if (command.Name.Trim().Length > 100)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Inventory item name must not exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}