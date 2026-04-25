using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Domain.M100_System.M102_Organization.CostCenters;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class CostCenterReadRepository : ICostCenterReadRepository
{
    private readonly AppDbContext _dbContext;

    public CostCenterReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.CostCenters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(code);
        return GetByCodeInternalAsync(normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<CostCenter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.CostCenters
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

    private async Task<CostCenter?> GetByCodeInternalAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.CostCenters
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items.FirstOrDefault(x =>
            string.Equals(x.Code.Value, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }
}
