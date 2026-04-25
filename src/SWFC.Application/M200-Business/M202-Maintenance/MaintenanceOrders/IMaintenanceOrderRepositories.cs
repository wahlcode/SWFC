using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;

public interface IMaintenanceOrderReadRepository
{
    Task<IReadOnlyList<MaintenanceOrder>> GetAllAsync(CancellationToken cancellationToken);
    Task<MaintenanceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> NumberExistsAsync(string number, Guid? excludeId, CancellationToken cancellationToken);
}

public interface IMaintenanceOrderWriteRepository
{
    Task AddAsync(MaintenanceOrder order, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
