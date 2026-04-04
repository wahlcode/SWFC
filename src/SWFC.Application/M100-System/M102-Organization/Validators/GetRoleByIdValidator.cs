using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Queries;

namespace SWFC.Application.M100_System.M102_Organization.Validators;

public sealed class GetRoleByIdValidator : ICommandValidator<GetRoleByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetRoleByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.RoleId == Guid.Empty)
        {
            result.Add("RoleId", "Role id is required.");
        }

        return Task.FromResult(result);
    }
}