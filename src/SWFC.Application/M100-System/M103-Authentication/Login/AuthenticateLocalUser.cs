using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

namespace SWFC.Application.M100_System.M103_Authentication;

public sealed record AuthenticateLocalUserCommand(
    string Username,
    string Password);

public sealed class AuthenticateLocalUserValidator : ICommandValidator<AuthenticateLocalUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        AuthenticateLocalUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.Username))
        {
            result.Add("Username", "Username is required.");
        }
        else
        {
            var normalizedUsername = command.Username.Trim();

            if (normalizedUsername.Length < 3)
            {
                result.Add("Username", "Username must be at least 3 characters long.");
            }

            if (normalizedUsername.Length > 100)
            {
                result.Add("Username", "Username must not exceed 100 characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            result.Add("Password", "Password is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class AuthenticateLocalUserHandler : IUseCaseHandler<AuthenticateLocalUserCommand, AuthenticationResultDto>
{
    private readonly ICommandValidator<AuthenticateLocalUserCommand> _validator;
    private readonly ILocalAuthenticationService _localAuthenticationService;

    public AuthenticateLocalUserHandler(
        ICommandValidator<AuthenticateLocalUserCommand> validator,
        ILocalAuthenticationService localAuthenticationService)
    {
        _validator = validator;
        _localAuthenticationService = localAuthenticationService;
    }

    public async Task<Result<AuthenticationResultDto>> HandleAsync(
        AuthenticateLocalUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(x => x.Message));

            return Result<AuthenticationResultDto>.Failure(
                new Error(
                    "m103.auth.validation_failed",
                    message,
                    ErrorCategory.Validation));
        }

        var result = await _localAuthenticationService.AuthenticateAsync(
            request.Username,
            request.Password,
            cancellationToken);

        return Result<AuthenticationResultDto>.Success(result);
    }
}

