using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Domain.Common.Errors;

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
            result.Add(ErrorCodes.Validation.Invalid, "Machine id is required.");
        }

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