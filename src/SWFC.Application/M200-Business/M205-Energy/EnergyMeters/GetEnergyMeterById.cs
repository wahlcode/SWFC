using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyMeters;

public sealed record GetEnergyMeterByIdQuery(Guid Id);

public sealed class GetEnergyMeterByIdPolicy : IAuthorizationPolicy<GetEnergyMeterByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetEnergyMeterByIdQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-meters.read" });
    }
}

public sealed class GetEnergyMeterByIdHandler
    : IUseCaseHandler<GetEnergyMeterByIdQuery, EnergyMeterDetailsDto>
{
    private readonly IEnergyMeterReadRepository _readRepository;

    public GetEnergyMeterByIdHandler(IEnergyMeterReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<EnergyMeterDetailsDto>> HandleAsync(
        GetEnergyMeterByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var meter = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (meter is null)
            throw new NotFoundException($"Energy meter '{request.Id}' was not found.");

        var dto = new EnergyMeterDetailsDto(
            meter.Id,
            meter.Name.Value,
            meter.MediumType,
            meter.Unit.Value,
            meter.IsManualEntryEnabled,
            meter.IsExternalImportEnabled,
            meter.ExternalSystem?.Value,
            meter.RfidTag?.Value,
            meter.SupportsOfflineCapture,
            meter.MachineId,
            meter.IsActive,
            meter.AuditInfo.CreatedAtUtc,
            meter.AuditInfo.CreatedBy,
            meter.AuditInfo.LastModifiedAtUtc,
            meter.AuditInfo.LastModifiedBy);

        return Result<EnergyMeterDetailsDto>.Success(dto);
    }
}
