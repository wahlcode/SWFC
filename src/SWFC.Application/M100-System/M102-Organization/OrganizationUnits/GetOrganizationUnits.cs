using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.OrganizationUnits;

public sealed record GetOrganizationUnitsQuery;

public sealed class GetOrganizationUnitsPolicy : IAuthorizationPolicy<GetOrganizationUnitsQuery>
{
    public AuthorizationRequirement GetRequirement(GetOrganizationUnitsQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetOrganizationUnitsHandler : IUseCaseHandler<GetOrganizationUnitsQuery, IReadOnlyList<OrganizationUnitListItem>>
{
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;

    public GetOrganizationUnitsHandler(IOrganizationUnitReadRepository organizationUnitReadRepository)
    {
        _organizationUnitReadRepository = organizationUnitReadRepository;
    }

    public async Task<Result<IReadOnlyList<OrganizationUnitListItem>>> HandleAsync(
        GetOrganizationUnitsQuery command,
        CancellationToken cancellationToken = default)
    {
        var organizationUnits = await _organizationUnitReadRepository.GetAllAsync(cancellationToken);
        var parentMap = organizationUnits.ToDictionary(x => x.Id);

        var items = organizationUnits
            .Select(x =>
            {
                string? parentName = null;
                string? parentCode = null;

                if (x.ParentOrganizationUnitId.HasValue &&
                    parentMap.TryGetValue(x.ParentOrganizationUnitId.Value, out var parent))
                {
                    parentName = parent.Name.Value;
                    parentCode = parent.Code.Value;
                }

                return new OrganizationUnitListItem(
                    x.Id,
                    x.Name.Value,
                    x.Code.Value,
                    x.ParentOrganizationUnitId,
                    parentName,
                    parentCode,
                    x.IsActive);
            })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<IReadOnlyList<OrganizationUnitListItem>>.Success(items);
    }
}
