using SWFC.Application.Common.Validation;
using SWFC.Domain.Common.Errors;
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
            result.Add(ErrorCodes.Machine.NameRequired, "Machine name is required.");
        }
        else if (command.Name.Trim().Length > 100)
        {
            result.Add(ErrorCodes.Machine.NameTooLong, "Machine name must not exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}