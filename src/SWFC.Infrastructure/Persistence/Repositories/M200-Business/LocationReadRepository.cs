using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M204_Inventory.Locations;
using SWFC.Application.M200_Business.M204_Inventory.Reservations;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class LocationReadRepository : ILocationReadRepository
{
    private readonly AppDbContext _dbContext;

    public LocationReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LocationListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _dbContext.Locations
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var parentMap = locations.ToDictionary(x => x.Id);

        return locations
            .OrderBy(x => x.Name.Value)
            .Select(x => new LocationListItem(
                x.Id,
                x.Name.Value,
                x.Code.Value,
                x.ParentLocationId,
                x.ParentLocationId.HasValue && parentMap.TryGetValue(x.ParentLocationId.Value, out var parent)
                    ? parent.Name.Value
                    : null,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();
    }

    public async Task<LocationDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (location is null)
        {
            return null;
        }

        string? parentName = null;

        if (location.ParentLocationId.HasValue)
        {
            parentName = await _dbContext.Locations
                .AsNoTracking()
                .Where(x => x.Id == location.ParentLocationId.Value)
                .Select(x => x.Name.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new LocationDetailsDto(
            location.Id,
            location.Name.Value,
            location.Code.Value,
            location.ParentLocationId,
            parentName,
            location.AuditInfo.CreatedAtUtc,
            location.AuditInfo.CreatedBy,
            location.AuditInfo.LastModifiedAtUtc,
            location.AuditInfo.LastModifiedBy);
    }

    public async Task<IReadOnlyList<LocationLookupItem>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .AsNoTracking()
            .OrderBy(x => x.Name.Value)
            .ThenBy(x => x.Code.Value)
            .Select(x => new LocationLookupItem(
                x.Id,
                x.Name.Value,
                x.Code.Value,
                x.ParentLocationId))
            .ToListAsync(cancellationToken);
    }
}

