using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;

namespace SWFC.Application.M200_Business.M201_Assets.Validators;

public sealed class UpdateMachineValidator : ICommandValidator<UpdateMachineCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Machine id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Machine name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.InventoryNumber))
        {
            result.Add("InventoryNumber", "Inventory number is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Status))
        {
            result.Add("Status", "Machine status is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}