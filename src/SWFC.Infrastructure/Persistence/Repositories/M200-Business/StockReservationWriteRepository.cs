using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockReservationWriteRepository : IStockReservationWriteRepository
{
    private readonly AppDbContext _dbContext;

    public StockReservationWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Stock?> GetStockByIdAsync(Guid stockId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == stockId, cancellationToken);
    }

    public Task<StockReservation?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StockReservations
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
    }

    public async Task AddAsync(StockReservation reservation, CancellationToken cancellationToken = default)
    {
        await _dbContext.StockReservations.AddAsync(reservation, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}