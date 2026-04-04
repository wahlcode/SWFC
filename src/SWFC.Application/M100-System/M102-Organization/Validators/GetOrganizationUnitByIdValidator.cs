using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Queries;

namespace SWFC.Application.M100_System.M102_Organization.Validators;

public sealed class GetOrganizationUnitByIdValidator : ICommandValidator<GetOrganizationUnitByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetOrganizationUnitByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.OrganizationUnitId == Guid.Empty)
        {
            result.Add("OrganizationUnitId", "Organization unit id is required.");
        }

        return Task.FromResult(result);
    }
}