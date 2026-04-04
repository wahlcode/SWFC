using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Commands;

namespace SWFC.Application.M100_System.M102_Organization.Validators;

public sealed class CreateOrganizationUnitValidator : ICommandValidator<CreateOrganizationUnitCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateOrganizationUnitCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Organization unit name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            result.Add("Code", "Organization unit code is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}