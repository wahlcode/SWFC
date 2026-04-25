using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M204_Inventory.Locations;
using SWFC.Application.M200_Business.M204_Inventory.Reservations;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
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
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.Id == stockId, cancellationToken);
    }

    public Task<StockReservation?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.StockReservations
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
    }

    public Task AddAsync(StockReservation reservation, CancellationToken cancellationToken = default)
    {
        return _dbContext.StockReservations.AddAsync(reservation, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

