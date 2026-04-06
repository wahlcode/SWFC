using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;

namespace SWFC.Application.M100_System.M103_Authentication.Validators;

public sealed class AuthenticateLocalUserValidator : ICommandValidator<AuthenticateLocalUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        AuthenticateLocalUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
        {
            throw new ArgumentException("Username is required.", nameof(command.Username));
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ArgumentException("Password is required.", nameof(command.Password));
        }

        return Task.FromResult<ValidationResult>(null!);
    }
}