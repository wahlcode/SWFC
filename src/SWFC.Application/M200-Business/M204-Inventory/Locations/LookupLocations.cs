using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public sealed record GetLocationLookupQuery;

public sealed class GetLocationLookupPolicy : IAuthorizationPolicy<GetLocationLookupQuery>
{
    public AuthorizationRequirement GetRequirement(GetLocationLookupQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "location.read" });
    }
}

public sealed class GetLocationLookupHandler : IUseCaseHandler<GetLocationLookupQuery, IReadOnlyList<LocationLookupItem>>
{
    private readonly ILocationReadRepository _locationReadRepository;

    public GetLocationLookupHandler(ILocationReadRepository locationReadRepository)
    {
        _locationReadRepository = locationReadRepository;
    }

    public async Task<Result<IReadOnlyList<LocationLookupItem>>> HandleAsync(
        GetLocationLookupQuery command,
        CancellationToken cancellationToken = default)
    {
        var items = await _locationReadRepository.GetLookupAsync(cancellationToken);
        return Result<IReadOnlyList<LocationLookupItem>>.Success(items);
    }
}

