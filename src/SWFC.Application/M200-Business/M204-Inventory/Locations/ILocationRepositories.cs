using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public interface ILocationReadRepository
{
    Task<IReadOnlyList<LocationListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LocationDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationLookupItem>> GetLookupAsync(CancellationToken cancellationToken = default);
}

public interface ILocationWriteRepository
{
    Task AddAsync(Location location, CancellationToken cancellationToken = default);
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

