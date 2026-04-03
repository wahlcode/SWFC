using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockReservationReadRepository : IStockReservationReadRepository
{
    private readonly AppDbContext _dbContext;

    public StockReservationReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StockReservationListItem>> GetAllAsync(
        Guid? stockId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockReservations
            .AsNoTracking()
            .AsQueryable();

        if (stockId.HasValue)
        {
            query = query.Where(x => x.StockId == stockId.Value);
        }

        var reservations = await query
            .OrderByDescending(x => x.AuditInfo.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return reservations
            .Select(x => new StockReservationListItem(
                x.Id,
                x.StockId,
                x.Quantity,
                x.Note,
                x.Status,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();
    }

    public async Task<StockReservationDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.StockReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        return new StockReservationDetailsDto(
            reservation.Id,
            reservation.StockId,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            reservation.AuditInfo.CreatedAtUtc,
            reservation.AuditInfo.CreatedBy,
            reservation.AuditInfo.LastModifiedAtUtc,
            reservation.AuditInfo.LastModifiedBy);
    }
}