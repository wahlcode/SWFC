using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M700_Support.M704_Incident_Management;

public sealed record GetIncidentsQuery;

public sealed class GetIncidentsPolicy : IAuthorizationPolicy<GetIncidentsQuery>
{
    public AuthorizationRequirement GetRequirement(GetIncidentsQuery request) =>
        new(requiredPermissions: new[] { "support.read" });
}

public sealed class GetIncidentsHandler : IUseCaseHandler<GetIncidentsQuery, IReadOnlyList<IncidentListItem>>
{
    private readonly IIncidentReadRepository _incidentReadRepository;

    public GetIncidentsHandler(IIncidentReadRepository incidentReadRepository)
    {
        _incidentReadRepository = incidentReadRepository;
    }

    public async Task<Result<IReadOnlyList<IncidentListItem>>> HandleAsync(
        GetIncidentsQuery command,
        CancellationToken cancellationToken = default)
    {
        var incidents = await _incidentReadRepository.GetAllAsync(cancellationToken);

        var items = incidents
            .OrderByDescending(x => x.AuditInfo.LastModifiedAtUtc ?? x.AuditInfo.CreatedAtUtc)
            .Select(x => new IncidentListItem(
                x.Id,
                x.Category,
                x.Description,
                x.Escalation,
                x.ReactionControl,
                x.NotificationReference,
                x.RuntimeReference,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<IncidentListItem>>.Success(items);
    }
}
