using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Queries;

namespace SWFC.Application.M100_System.M102_Organization.Validators;

public sealed class GetUserByIdValidator : ICommandValidator<GetUserByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetUserByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        return Task.FromResult(result);
    }
}