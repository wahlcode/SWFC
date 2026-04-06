using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;

namespace SWFC.Application.M100_System.M103_Authentication.Validators;

public sealed class ChangeOwnPasswordValidator : ICommandValidator<ChangeOwnPasswordCommand>
{
    private const int MinimumPasswordLength = 12;

    public Task<ValidationResult> ValidateAsync(
        ChangeOwnPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            throw new ArgumentException("Current password is required.", nameof(command.CurrentPassword));
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            throw new ArgumentException("New password is required.", nameof(command.NewPassword));
        }

        if (command.NewPassword.Trim().Length < MinimumPasswordLength)
        {
            throw new ArgumentException(
                $"New password must be at least {MinimumPasswordLength} characters long.",
                nameof(command.NewPassword));
        }

        return Task.FromResult<ValidationResult>(null!);
    }
}