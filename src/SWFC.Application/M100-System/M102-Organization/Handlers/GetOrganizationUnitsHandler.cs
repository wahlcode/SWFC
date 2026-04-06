using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

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
        var organizationUnitsById = organizationUnits.ToDictionary(x => x.Id);

        var items = organizationUnits
            .Select(x =>
            {
                var parent = x.ParentOrganizationUnitId.HasValue &&
                             organizationUnitsById.TryGetValue(x.ParentOrganizationUnitId.Value, out var parentOrganizationUnit)
                    ? parentOrganizationUnit
                    : null;

                return new OrganizationUnitListItem(
                    x.Id,
                    x.Name.Value,
                    x.Code.Value,
                    x.ParentOrganizationUnitId,
                    parent?.Name.Value,
                    parent?.Code.Value);
            })
            .ToList();

        return Result<IReadOnlyList<OrganizationUnitListItem>>.Success(items);
    }
}