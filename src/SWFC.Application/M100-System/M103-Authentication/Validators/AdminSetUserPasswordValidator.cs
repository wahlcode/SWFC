using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;

namespace SWFC.Application.M100_System.M103_Authentication.Validators;

public sealed class AdminSetUserPasswordValidator : ICommandValidator<AdminSetUserPasswordCommand>
{
    private const int MinimumPasswordLength = 12;

    public Task<ValidationResult> ValidateAsync(
        AdminSetUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(command.UserId));
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