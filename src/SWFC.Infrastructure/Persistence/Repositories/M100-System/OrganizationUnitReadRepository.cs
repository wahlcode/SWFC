using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Domain.M100_System.M102_Organization.Entities;
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
        var organizationUnit = _dbContext.OrganizationUnits
            .AsNoTracking()
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Code.Value, code, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(organizationUnit);
    }

    public Task<IReadOnlyList<OrganizationUnit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrganizationUnit> organizationUnits = _dbContext.OrganizationUnits
            .AsNoTracking()
            .AsEnumerable()
            .OrderBy(x => x.Name.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(organizationUnits);
    }
}