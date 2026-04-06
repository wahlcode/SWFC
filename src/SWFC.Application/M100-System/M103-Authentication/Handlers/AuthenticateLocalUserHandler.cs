using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;
using SWFC.Application.M100_System.M103_Authentication.DTOs;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M103_Authentication.Handlers;

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
        await _validator.ValidateAsync(request, cancellationToken);

        var result = await _localAuthenticationService.AuthenticateAsync(
            request.Username,
            request.Password,
            cancellationToken);

        return Result<AuthenticationResultDto>.Success(result);
    }
}