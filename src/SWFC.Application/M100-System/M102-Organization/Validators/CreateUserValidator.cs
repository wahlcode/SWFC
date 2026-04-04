using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Commands;

namespace SWFC.Application.M100_System.M102_Organization.Validators;

public sealed class CreateUserValidator : ICommandValidator<CreateUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.IdentityKey))
        {
            result.Add("IdentityKey", "Identity key is required.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            result.Add("DisplayName", "Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}