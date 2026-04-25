using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyMeters;

public sealed record GetEnergyMetersQuery;

public sealed class GetEnergyMetersPolicy : IAuthorizationPolicy<GetEnergyMetersQuery>
{
    public AuthorizationRequirement GetRequirement(GetEnergyMetersQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-meters.read" });
    }
}

public sealed class GetEnergyMetersHandler
    : IUseCaseHandler<GetEnergyMetersQuery, IReadOnlyList<EnergyMeterListItemDto>>
{
    private readonly IEnergyMeterReadRepository _readRepository;

    public GetEnergyMetersHandler(IEnergyMeterReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<IReadOnlyList<EnergyMeterListItemDto>>> HandleAsync(
        GetEnergyMetersQuery request,
        CancellationToken cancellationToken = default)
    {
        var meters = await _readRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<EnergyMeterListItemDto> items = meters
            .OrderBy(x => x.Name.Value)
            .Select(x => new EnergyMeterListItemDto(
                x.Id,
                x.Name.Value,
                x.MediumType,
                x.Unit.Value,
                x.IsManualEntryEnabled,
                x.IsExternalImportEnabled,
                x.ExternalSystem?.Value,
                x.RfidTag?.Value,
                x.SupportsOfflineCapture,
                x.MachineId,
                x.IsActive,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<EnergyMeterListItemDto>>.Success(items);
    }
}
