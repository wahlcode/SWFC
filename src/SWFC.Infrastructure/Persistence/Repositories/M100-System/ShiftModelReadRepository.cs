using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Domain.M100_System.M102_Organization.ShiftModels;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class ShiftModelReadRepository : IShiftModelReadRepository
{
    private readonly AppDbContext _dbContext;

    public ShiftModelReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShiftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShiftModels
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<ShiftModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(code);
        return GetByCodeInternalAsync(normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<ShiftModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.ShiftModels
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items
            .OrderBy(x => x.Name.Value)
            .ThenBy(x => x.Code.Value)
            .ToList();
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim();
    }

    private async Task<ShiftModel?> GetByCodeInternalAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.ShiftModels
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items.FirstOrDefault(x =>
            string.Equals(x.Code.Value, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }
}
