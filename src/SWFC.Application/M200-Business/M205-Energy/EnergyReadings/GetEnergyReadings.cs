using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyReadings;

public sealed record GetEnergyReadingsQuery(Guid MeterId);

public sealed class GetEnergyReadingsPolicy : IAuthorizationPolicy<GetEnergyReadingsQuery>
{
    public AuthorizationRequirement GetRequirement(GetEnergyReadingsQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-readings.read" });
    }
}

public sealed class GetEnergyReadingsHandler
    : IUseCaseHandler<GetEnergyReadingsQuery, IReadOnlyList<EnergyReadingListItemDto>>
{
    private readonly IEnergyReadingReadRepository _readRepository;

    public GetEnergyReadingsHandler(IEnergyReadingReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<IReadOnlyList<EnergyReadingListItemDto>>> HandleAsync(
        GetEnergyReadingsQuery request,
        CancellationToken cancellationToken = default)
    {
        var readings = await _readRepository.GetByMeterIdAsync(request.MeterId, cancellationToken);

        IReadOnlyList<EnergyReadingListItemDto> items = readings
            .OrderByDescending(x => x.Date.Value)
            .ThenByDescending(x => x.AuditInfo.CreatedAtUtc)
            .Select(x => new EnergyReadingListItemDto(
                x.Id,
                x.MeterId,
                x.Date.Value,
                x.Value.Value,
                x.Source,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<EnergyReadingListItemDto>>.Success(items);
    }
}
