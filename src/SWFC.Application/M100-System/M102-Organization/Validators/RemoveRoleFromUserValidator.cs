using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Commands;

namespace SWFC.Application.M100_System.M102_Organization.Validators;

public sealed class RemoveRoleFromUserValidator : ICommandValidator<RemoveRoleFromUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        RemoveRoleFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        if (command.RoleId == Guid.Empty)
        {
            result.Add("RoleId", "Role id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}