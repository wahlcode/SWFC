using System.Text.Json;
using SWFC.Application.M700_Support.M704_Incident_Management;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M704_Incident_Management;

namespace SWFC.Application.M700_Support.M703_SupportCases;

public sealed record EscalateSupportCaseToIncidentCommand(
    Guid SupportCaseId,
    IncidentCategory Category,
    string Escalation,
    string ReactionControl,
    string? NotificationReference,
    string? RuntimeReference,
    string Reason);

public sealed class EscalateSupportCaseToIncidentValidator : ICommandValidator<EscalateSupportCaseToIncidentCommand>
{
    public Task<ValidationResult> ValidateAsync(
        EscalateSupportCaseToIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.SupportCaseId == Guid.Empty)
        {
            result.Add("SupportCaseId", "Support case id is required.");
        }

        if (!Enum.IsDefined(command.Category))
        {
            result.Add("Category", "Category is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Escalation))
        {
            result.Add("Escalation", "Escalation is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ReactionControl))
        {
            result.Add("ReactionControl", "Reaction control is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class EscalateSupportCaseToIncidentPolicy : IAuthorizationPolicy<EscalateSupportCaseToIncidentCommand>
{
    public AuthorizationRequirement GetRequirement(EscalateSupportCaseToIncidentCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class EscalateSupportCaseToIncidentHandler : IUseCaseHandler<EscalateSupportCaseToIncidentCommand, Guid>
{
    private readonly ISupportCaseWriteRepository _supportCaseWriteRepository;
    private readonly IIncidentWriteRepository _incidentWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public EscalateSupportCaseToIncidentHandler(
        ISupportCaseWriteRepository supportCaseWriteRepository,
        IIncidentWriteRepository incidentWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _supportCaseWriteRepository = supportCaseWriteRepository;
        _incidentWriteRepository = incidentWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        EscalateSupportCaseToIncidentCommand command,
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

        var incident = Incident.Create(
            command.Category,
            supportCase.ProblemDescription,
            command.Escalation,
            command.ReactionControl,
            command.NotificationReference,
            command.RuntimeReference,
            changeContext);

        await _incidentWriteRepository.AddAsync(incident, cancellationToken);
        supportCase.LinkIncident(incident.Id, changeContext);

        await _incidentWriteRepository.SaveChangesAsync(cancellationToken);
        await _supportCaseWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "EscalateSupportCaseToIncident",
                Module: "M703",
                ObjectType: "SupportCase",
                ObjectId: supportCase.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    supportCase.Id,
                    supportCase.TriggeredIncidentId,
                    CreatedIncidentId = incident.Id
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(incident.Id);
    }
}
