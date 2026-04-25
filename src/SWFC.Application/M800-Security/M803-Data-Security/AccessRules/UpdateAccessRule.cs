using System.Text.Json;
using SWFC.Application.M800_Security.M801_Access;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Application.M800_Security.M803_Visibility.AccessRules;

public sealed record UpdateAccessRuleCommand(
    Guid Id,
    AccessRuleMode Mode,
    string Reason);

public sealed class UpdateAccessRuleValidator : ICommandValidator<UpdateAccessRuleCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateAccessRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Rule id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateAccessRulePolicy : IAuthorizationPolicy<UpdateAccessRuleCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateAccessRuleCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class UpdateAccessRuleHandler : IUseCaseHandler<UpdateAccessRuleCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccessRuleWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public UpdateAccessRuleHandler(
        ICurrentUserService currentUserService,
        IAccessRuleWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateAccessRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        var rule = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (rule is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Access rule '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            rule.Id,
            rule.TargetType,
            rule.TargetId,
            rule.SubjectType,
            rule.SubjectId,
            rule.Mode
        });

        rule.UpdateMode(command.Mode, changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            rule.Id,
            rule.TargetType,
            rule.TargetId,
            rule.SubjectType,
            rule.SubjectId,
            rule.Mode,
            command.Reason
        });

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "UpdateAccessRule",
            "AccessRule",
            rule.Id.ToString(),
            changeContext.ChangedAtUtc,
            oldValues,
            newValues,
            cancellationToken);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
