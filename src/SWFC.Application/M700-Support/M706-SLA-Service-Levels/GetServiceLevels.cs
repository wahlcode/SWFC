using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M700_Support.M706_SLA_Service_Levels;

public sealed record GetServiceLevelsQuery;

public sealed class GetServiceLevelsPolicy : IAuthorizationPolicy<GetServiceLevelsQuery>
{
    public AuthorizationRequirement GetRequirement(GetServiceLevelsQuery request) =>
        new(requiredPermissions: new[] { "support.read" });
}

public sealed class GetServiceLevelsHandler : IUseCaseHandler<GetServiceLevelsQuery, IReadOnlyList<ServiceLevelListItem>>
{
    private readonly IServiceLevelReadRepository _serviceLevelReadRepository;

    public GetServiceLevelsHandler(IServiceLevelReadRepository serviceLevelReadRepository)
    {
        _serviceLevelReadRepository = serviceLevelReadRepository;
    }

    public async Task<Result<IReadOnlyList<ServiceLevelListItem>>> HandleAsync(
        GetServiceLevelsQuery command,
        CancellationToken cancellationToken = default)
    {
        var serviceLevels = await _serviceLevelReadRepository.GetAllAsync(cancellationToken);

        var items = serviceLevels
            .OrderByDescending(x => x.AuditInfo.LastModifiedAtUtc ?? x.AuditInfo.CreatedAtUtc)
            .Select(x => new ServiceLevelListItem(
                x.Id,
                x.Priority,
                x.ResponseTime,
                x.ProcessingTime,
                x.UseForSupport,
                x.UseForIncidentManagement,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<ServiceLevelListItem>>.Success(items);
    }
}
