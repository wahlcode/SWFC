using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M103_Authentication.Handlers;

public sealed class AdminSetUserPasswordHandler : IUseCaseHandler<AdminSetUserPasswordCommand, bool>
{
    private readonly ICommandValidator<AdminSetUserPasswordCommand> _validator;
    private readonly ILocalAuthenticationService _localAuthenticationService;

    public AdminSetUserPasswordHandler(
        ICommandValidator<AdminSetUserPasswordCommand> validator,
        ILocalAuthenticationService localAuthenticationService)
    {
        _validator = validator;
        _localAuthenticationService = localAuthenticationService;
    }

    public async Task<Result<bool>> HandleAsync(
        AdminSetUserPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAsync(request, cancellationToken);

        try
        {
            await _localAuthenticationService.AdminSetPasswordAsync(
                request.UserId,
                request.NewPassword,
                cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(
                new Error(
                    "m103.auth.admin_set_password_failed",
                    ex.Message,
                    ErrorCategory.Security));
        }
    }
}