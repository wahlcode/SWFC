using SWFC.Domain.M100_System.M102_Organization.ShiftModels;

namespace SWFC.Application.M100_System.M102_Organization.ShiftModels;

public interface IShiftModelReadRepository
{
    Task<ShiftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShiftModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShiftModel>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IShiftModelWriteRepository
{
    Task AddAsync(ShiftModel shiftModel, CancellationToken cancellationToken = default);
    Task<ShiftModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShiftModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
