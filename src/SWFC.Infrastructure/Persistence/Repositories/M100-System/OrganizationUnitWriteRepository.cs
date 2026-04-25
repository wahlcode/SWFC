using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class OrganizationUnitWriteRepository : IOrganizationUnitWriteRepository
{
    private readonly AppDbContext _dbContext;

    public OrganizationUnitWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OrganizationUnit unit, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganizationUnits.AddAsync(unit, cancellationToken).AsTask();
    }

    public Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganizationUnits
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
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

    private async Task<OrganizationUnit?> GetByCodeInternalAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.OrganizationUnits.ToListAsync(cancellationToken);

        return items.FirstOrDefault(x =>
            string.Equals(x.Code.Value, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }
}
