using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;

public interface IMaintenancePlanReadRepository
{
    Task<IReadOnlyList<MaintenancePlan>> GetAllAsync(CancellationToken cancellationToken);
    Task<MaintenancePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

public interface IMaintenancePlanWriteRepository
{
    Task AddAsync(MaintenancePlan plan, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
