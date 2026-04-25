using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyReadings;

public sealed record EnergyReadingListItemDto(
    Guid Id,
    Guid MeterId,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record EnergyReadingDetailsDto(
    Guid Id,
    Guid MeterId,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
