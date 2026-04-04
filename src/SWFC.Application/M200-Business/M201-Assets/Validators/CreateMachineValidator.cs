using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;

namespace SWFC.Application.M200_Business.M201_Assets.Validators;

public sealed class CreateMachineValidator : ICommandValidator<CreateMachineCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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