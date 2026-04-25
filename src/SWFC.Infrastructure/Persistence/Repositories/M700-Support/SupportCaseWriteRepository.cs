using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M703_SupportCases;
using SWFC.Domain.M700_Support.M703_SupportCases;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class SupportCaseWriteRepository : ISupportCaseWriteRepository
{
    private readonly AppDbContext _dbContext;

    public SupportCaseWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(SupportCase supportCase, CancellationToken cancellationToken = default)
    {
        await _dbContext.SupportCases.AddAsync(supportCase, cancellationToken);
    }

    public Task<SupportCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.SupportCases.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
