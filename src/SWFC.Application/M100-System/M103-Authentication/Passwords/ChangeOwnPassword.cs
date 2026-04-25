using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M103_Authentication;

public sealed record ChangeOwnPasswordCommand(
    string CurrentPassword,
    string NewPassword);

public sealed class ChangeOwnPasswordValidator : ICommandValidator<ChangeOwnPasswordCommand>
{
    private const int MinimumPasswordLength = 12;

    public Task<ValidationResult> ValidateAsync(
        ChangeOwnPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            result.Add("CurrentPassword", "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            result.Add("NewPassword", "New password is required.");
        }
        else
        {
            var normalizedNewPassword = command.NewPassword.Trim();

            if (normalizedNewPassword.Length < MinimumPasswordLength)
            {
                result.Add(
                    "NewPassword",
                    $"New password must be at least {MinimumPasswordLength} characters long.");
            }

            if (!string.IsNullOrWhiteSpace(command.CurrentPassword) &&
                string.Equals(
                    command.CurrentPassword.Trim(),
                    normalizedNewPassword,
                    StringComparison.Ordinal))
            {
                result.Add(
                    "NewPassword",
                    "New password must be different from the current password.");
            }
        }

        return Task.FromResult(result);
    }
}

public sealed class ChangeOwnPasswordHandler : IUseCaseHandler<ChangeOwnPasswordCommand, bool>
{
    private readonly ICommandValidator<ChangeOwnPasswordCommand> _validator;
    private readonly ILocalAuthenticationService _localAuthenticationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public ChangeOwnPasswordHandler(
        ICommandValidator<ChangeOwnPasswordCommand> validator,
        ILocalAuthenticationService localAuthenticationService,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _validator = validator;
        _localAuthenticationService = localAuthenticationService;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        ChangeOwnPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(x => x.Message));

            return Result<bool>.Failure(
                new Error(
                    "m103.auth.invalid_input",
                    message,
                    ErrorCategory.Validation));
        }

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
            var timestampUtc = DateTime.UtcNow;

            await _localAuthenticationService.ChangeOwnPasswordAsync(
                userId,
                request.CurrentPassword,
                request.NewPassword,
                cancellationToken);

            await _auditService.WriteAsync(
                new AuditWriteRequest(
                    ActorUserId: securityContext.UserId,
                    ActorDisplayName: securityContext.DisplayName,
                    Action: "PasswordChanged",
                    Module: "M103",
                    ObjectType: "LocalCredential",
                    ObjectId: userId.ToString(),
                    TimestampUtc: timestampUtc,
                    TargetUserId: userId.ToString(),
                    Reason: "Own password changed."),
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
