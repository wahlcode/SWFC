using System.Text.Json;
using SWFC.Application.M800_Security.M801_Access;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Application.M800_Security.M803_Visibility.AccessRules;

public sealed record CreateAccessRuleCommand(
    AccessTargetType TargetType,
    string TargetId,
    AccessSubjectType SubjectType,
    string SubjectId,
    AccessRuleMode Mode,
    string Reason);

public sealed class CreateAccessRuleValidator : ICommandValidator<CreateAccessRuleCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateAccessRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.TargetId))
        {
            result.Add("TargetId", "Target id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.SubjectId))
        {
            result.Add("SubjectId", "Subject id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateAccessRulePolicy : IAuthorizationPolicy<CreateAccessRuleCommand>
{
    public AuthorizationRequirement GetRequirement(CreateAccessRuleCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class CreateAccessRuleHandler : IUseCaseHandler<CreateAccessRuleCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccessRuleWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public CreateAccessRuleHandler(
        ICurrentUserService currentUserService,
        IAccessRuleWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateAccessRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var rule = AccessRule.Create(
            command.TargetType,
            command.TargetId,
            command.SubjectType,
            command.SubjectId,
            command.Mode,
            changeContext);

        await _writeRepository.AddAsync(rule, cancellationToken);

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "CreateAccessRule",
            "AccessRule",
            rule.Id.ToString(),
            changeContext.ChangedAtUtc,
            null,
            JsonSerializer.Serialize(new
            {
                rule.Id,
                rule.TargetType,
                rule.TargetId,
                rule.SubjectType,
                rule.SubjectId,
                rule.Mode,
                command.Reason
            }),
            cancellationToken);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(rule.Id);
    }
}
