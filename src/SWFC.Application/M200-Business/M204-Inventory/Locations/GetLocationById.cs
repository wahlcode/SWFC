using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public sealed record GetLocationByIdQuery(Guid Id);

public sealed class GetLocationByIdPolicy : IAuthorizationPolicy<GetLocationByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetLocationByIdQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "location.read" });
    }
}

public sealed class GetLocationByIdHandler : IUseCaseHandler<GetLocationByIdQuery, LocationDetailsDto>
{
    private readonly ILocationReadRepository _locationReadRepository;

    public GetLocationByIdHandler(ILocationReadRepository locationReadRepository)
    {
        _locationReadRepository = locationReadRepository;
    }

    public async Task<Result<LocationDetailsDto>> HandleAsync(
        GetLocationByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var location = await _locationReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (location is null)
        {
            return Result<LocationDetailsDto>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Location '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<LocationDetailsDto>.Success(location);
    }
}

