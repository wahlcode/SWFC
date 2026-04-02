using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M201_Assets.Validators;

public sealed class GetMachineByIdValidator : ICommandValidator<GetMachineByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetMachineByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Machine id is required.");
        }

        return Task.FromResult(result);
    }
}