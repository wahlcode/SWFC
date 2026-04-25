using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M103_Authentication;

public sealed record AdminSetUserPasswordCommand(
    Guid UserId,
    string NewPassword);

public sealed class AdminSetUserPasswordValidator : ICommandValidator<AdminSetUserPasswordCommand>
{
    private const int MinimumPasswordLength = 12;

    public Task<ValidationResult> ValidateAsync(
        AdminSetUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
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
        }

        return Task.FromResult(result);
    }
}

public sealed class AdminSetUserPasswordHandler : IUseCaseHandler<AdminSetUserPasswordCommand, bool>
{
    private readonly ICommandValidator<AdminSetUserPasswordCommand> _validator;
    private readonly ILocalAuthenticationService _authenticationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadRepository _userReadRepository;
    private readonly ISecurityAdministrationGuard _securityAdministrationGuard;
    private readonly IAuditService _auditService;

    public AdminSetUserPasswordHandler(
        ICommandValidator<AdminSetUserPasswordCommand> validator,
        ILocalAuthenticationService authenticationService,
        ICurrentUserService currentUserService,
        IUserReadRepository userReadRepository,
        ISecurityAdministrationGuard securityAdministrationGuard,
        IAuditService auditService)
    {
        _validator = validator;
        _authenticationService = authenticationService;
        _currentUserService = currentUserService;
        _userReadRepository = userReadRepository;
        _securityAdministrationGuard = securityAdministrationGuard;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        AdminSetUserPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(x => x.Message));

            return Result<bool>.Failure(
                new Error(
                    "m103.auth.validation_failed",
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

        var passwordResetError = await _securityAdministrationGuard.ValidateAdminPasswordResetAsync(
            securityContext,
            request.UserId,
            cancellationToken);

        if (passwordResetError is not null)
        {
            return Result<bool>.Failure(passwordResetError);
        }

        try
        {
            var timestampUtc = DateTime.UtcNow;

            await _authenticationService.AdminSetPasswordAsync(
                request.UserId,
                request.NewPassword,
                cancellationToken);

            var targetUser = await _userReadRepository.GetByIdAsync(request.UserId, cancellationToken);

            await _auditService.WriteAsync(
                new AuditWriteRequest(
                    ActorUserId: securityContext.UserId,
                    ActorDisplayName: securityContext.DisplayName,
                    Action: "PasswordSet",
                    Module: "M103",
                    ObjectType: "LocalCredential",
                    ObjectId: request.UserId.ToString(),
                    TimestampUtc: timestampUtc,
                    NewValues: JsonSerializer.Serialize(new
                    {
                        ChangeType = "AdminPasswordReset",
                        UserId = request.UserId,
                        Username = targetUser?.Username.Value
                    }),
                    TargetUserId: request.UserId.ToString(),
                    Reason: "Password set by administrator."),
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
