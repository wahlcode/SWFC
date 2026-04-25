using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M704_Incident_Management;

namespace SWFC.Application.M700_Support.M704_Incident_Management;

public sealed record CreateIncidentCommand(
    IncidentCategory Category,
    string Description,
    string Escalation,
    string ReactionControl,
    string? NotificationReference,
    string? RuntimeReference,
    string Reason);

public sealed class CreateIncidentValidator : ICommandValidator<CreateIncidentCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (!Enum.IsDefined(command.Category))
        {
            result.Add("Category", "Category is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            result.Add("Description", "Description is required.");
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

public sealed class CreateIncidentPolicy : IAuthorizationPolicy<CreateIncidentCommand>
{
    public AuthorizationRequirement GetRequirement(CreateIncidentCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class CreateIncidentHandler : IUseCaseHandler<CreateIncidentCommand, Guid>
{
    private readonly IIncidentWriteRepository _incidentWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateIncidentHandler(
        IIncidentWriteRepository incidentWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _incidentWriteRepository = incidentWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var incident = Incident.Create(
            command.Category,
            command.Description,
            command.Escalation,
            command.ReactionControl,
            command.NotificationReference,
            command.RuntimeReference,
            changeContext);

        await _incidentWriteRepository.AddAsync(incident, cancellationToken);
        await _incidentWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "CreateIncident",
                Module: "M704",
                ObjectType: "Incident",
                ObjectId: incident.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    incident.Id,
                    Category = incident.Category.ToString(),
                    incident.Description,
                    incident.Escalation,
                    incident.ReactionControl,
                    incident.NotificationReference,
                    incident.RuntimeReference
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(incident.Id);
    }
}
