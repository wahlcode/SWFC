using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public sealed record GetLocationsQuery;

public sealed class GetLocationsPolicy : IAuthorizationPolicy<GetLocationsQuery>
{
    public AuthorizationRequirement GetRequirement(GetLocationsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "location.read" });
    }
}

public sealed class GetLocationsHandler : IUseCaseHandler<GetLocationsQuery, IReadOnlyList<LocationListItem>>
{
    private readonly ILocationReadRepository _locationReadRepository;

    public GetLocationsHandler(ILocationReadRepository locationReadRepository)
    {
        _locationReadRepository = locationReadRepository;
    }

    public async Task<Result<IReadOnlyList<LocationListItem>>> HandleAsync(
        GetLocationsQuery command,
        CancellationToken cancellationToken = default)
    {
        var locations = await _locationReadRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<LocationListItem>>.Success(locations);
    }
}

