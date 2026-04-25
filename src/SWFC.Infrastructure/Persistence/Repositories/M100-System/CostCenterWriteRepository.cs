using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Domain.M100_System.M102_Organization.CostCenters;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class CostCenterWriteRepository : ICostCenterWriteRepository
{
    private readonly AppDbContext _dbContext;

    public CostCenterWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(CostCenter costCenter, CancellationToken cancellationToken = default)
    {
        return _dbContext.CostCenters.AddAsync(costCenter, cancellationToken).AsTask();
    }

    public Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.CostCenters
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
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

    private async Task<CostCenter?> GetByCodeInternalAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.CostCenters.ToListAsync(cancellationToken);

        return items.FirstOrDefault(x =>
            string.Equals(x.Code.Value, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }
}
