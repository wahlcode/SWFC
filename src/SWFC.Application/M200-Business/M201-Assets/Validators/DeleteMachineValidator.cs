using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M201_Assets.Validators;

public sealed class DeleteMachineValidator : ICommandValidator<DeleteMachineCommand>
{
    public Task<ValidationResult> ValidateAsync(
        DeleteMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Machine id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(ErrorCodes.General.ContextRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}