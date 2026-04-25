using System.Text.Json;
using SWFC.Application.M700_Support.M701_BugTracking;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M701_BugTracking;

namespace SWFC.Application.M700_Support.M703_SupportCases;

public sealed record EscalateSupportCaseToBugCommand(
    Guid SupportCaseId,
    string Reproducibility,
    string Reason);

public sealed class EscalateSupportCaseToBugValidator : ICommandValidator<EscalateSupportCaseToBugCommand>
{
    public Task<ValidationResult> ValidateAsync(
        EscalateSupportCaseToBugCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.SupportCaseId == Guid.Empty)
        {
            result.Add("SupportCaseId", "Support case id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reproducibility))
        {
            result.Add("Reproducibility", "Reproducibility is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class EscalateSupportCaseToBugPolicy : IAuthorizationPolicy<EscalateSupportCaseToBugCommand>
{
    public AuthorizationRequirement GetRequirement(EscalateSupportCaseToBugCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class EscalateSupportCaseToBugHandler : IUseCaseHandler<EscalateSupportCaseToBugCommand, Guid>
{
    private readonly ISupportCaseWriteRepository _supportCaseWriteRepository;
    private readonly IBugWriteRepository _bugWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public EscalateSupportCaseToBugHandler(
        ISupportCaseWriteRepository supportCaseWriteRepository,
        IBugWriteRepository bugWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _supportCaseWriteRepository = supportCaseWriteRepository;
        _bugWriteRepository = bugWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        EscalateSupportCaseToBugCommand command,
        CancellationToken cancellationToken = default)
    {
        var supportCase = await _supportCaseWriteRepository.GetByIdAsync(command.SupportCaseId, cancellationToken);

        if (supportCase is null)
        {
            return Result<Guid>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"SupportCase '{command.SupportCaseId}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var bug = Bug.Create(
            supportCase.ProblemDescription,
            command.Reproducibility,
            BugStatus.Open,
            changeContext);

        await _bugWriteRepository.AddAsync(bug, cancellationToken);
        supportCase.LinkBug(bug.Id, changeContext);

        await _bugWriteRepository.SaveChangesAsync(cancellationToken);
        await _supportCaseWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "EscalateSupportCaseToBug",
                Module: "M703",
                ObjectType: "SupportCase",
                ObjectId: supportCase.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    supportCase.Id,
                    supportCase.TriggeredBugId,
                    CreatedBugId = bug.Id
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(bug.Id);
    }
}
