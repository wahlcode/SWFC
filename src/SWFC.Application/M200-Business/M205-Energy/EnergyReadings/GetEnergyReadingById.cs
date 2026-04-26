using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyReadings;

public sealed record GetEnergyReadingByIdQuery(Guid Id);

public sealed class GetEnergyReadingByIdPolicy : IAuthorizationPolicy<GetEnergyReadingByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetEnergyReadingByIdQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-readings.read" });
    }
}

public sealed class GetEnergyReadingByIdHandler
    : IUseCaseHandler<GetEnergyReadingByIdQuery, EnergyReadingDetailsDto>
{
    private readonly IEnergyReadingReadRepository _readRepository;

    public GetEnergyReadingByIdHandler(IEnergyReadingReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<EnergyReadingDetailsDto>> HandleAsync(
        GetEnergyReadingByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var reading = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (reading is null)
            throw new NotFoundException($"Energy reading '{request.Id}' was not found.");

        var dto = new EnergyReadingDetailsDto(
            reading.Id,
            reading.MeterId,
            reading.Date.Value,
            reading.Value.Value,
            reading.Source,
            reading.CapturedByUserId,
            reading.CaptureContext?.Value,
            reading.RfidTag?.Value,
            reading.RfidExceptionReason?.Value,
            reading.OfflineCaptureId,
            reading.CapturedOfflineAtUtc,
            reading.SyncedAtUtc,
            reading.PlausibilityStatus,
            reading.PlausibilityNote?.Value,
            reading.AuditInfo.CreatedAtUtc,
            reading.AuditInfo.CreatedBy,
            reading.AuditInfo.LastModifiedAtUtc,
            reading.AuditInfo.LastModifiedBy);

        return Result<EnergyReadingDetailsDto>.Success(dto);
    }
}
