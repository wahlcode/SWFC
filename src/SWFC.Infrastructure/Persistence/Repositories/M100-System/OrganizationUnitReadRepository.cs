using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class OrganizationUnitReadRepository : IOrganizationUnitReadRepository
{
    private readonly AppDbContext _dbContext;

    public OrganizationUnitReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganizationUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(code);
        return GetByCodeInternalAsync(normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationUnit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.OrganizationUnits
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

    private async Task<OrganizationUnit?> GetByCodeInternalAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.OrganizationUnits
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items.FirstOrDefault(x =>
            string.Equals(x.Code.Value, normalizedCode, StringComparison.OrdinalIgnoreCase));
    }
}
