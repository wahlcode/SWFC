using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Domain.M100_System.M102_Organization.ShiftModels;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class ShiftModelWriteRepository : IShiftModelWriteRepository
{
    private readonly AppDbContext _dbContext;

    public ShiftModelWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ShiftModel shiftModel, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShiftModels.AddAsync(shiftModel, cancellationToken).AsTask();
    }

    public Task<ShiftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShiftModels
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<ShiftModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(code);
        return GetByCodeInternalAsync(normalizedCode, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim();
    }

    private async Task<ShiftModel?> GetByCodeInternalAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.ShiftModels.ToListAsync(cancellationToken);

        return items.FirstOrDefault(x =>
            string.Equals(x.Code.Value, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }
}
