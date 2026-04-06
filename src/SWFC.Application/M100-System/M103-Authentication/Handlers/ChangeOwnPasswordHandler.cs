using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M103_Authentication.Handlers;

public sealed class ChangeOwnPasswordHandler : IUseCaseHandler<ChangeOwnPasswordCommand, bool>
{
    private readonly ICommandValidator<ChangeOwnPasswordCommand> _validator;
    private readonly ILocalAuthenticationService _localAuthenticationService;
    private readonly ICurrentUserService _currentUserService;

    public ChangeOwnPasswordHandler(
        ICommandValidator<ChangeOwnPasswordCommand> validator,
        ILocalAuthenticationService localAuthenticationService,
        ICurrentUserService currentUserService)
    {
        _validator = validator;
        _localAuthenticationService = localAuthenticationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> HandleAsync(
        ChangeOwnPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAsync(request, cancellationToken);

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        if (!securityContext.IsAuthenticated)
        {
            return Result<bool>.Failure(
                new Error(
                    "m103.auth.not_authenticated",
                    "User is not authenticated.",
                    ErrorCategory.Security));
        }

        if (!Guid.TryParse(securityContext.UserId, out var userId) || userId == Guid.Empty)
        {
            return Result<bool>.Failure(
                new Error(
                    "m103.auth.invalid_user_id",
                    "Authenticated user id is invalid.",
                    ErrorCategory.Security));
        }

        try
        {
            await _localAuthenticationService.ChangeOwnPasswordAsync(
                userId,
                request.CurrentPassword,
                request.NewPassword,
                cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(
                new Error(
                    "m103.auth.change_password_failed",
                    ex.Message,
                    ErrorCategory.Security));
        }
    }
}