using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;

namespace SWFC.Application.M200_Business.M206_Purchasing.Suppliers;

public interface ISupplierWriteRepository
{
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
}

public interface ISupplierReadRepository
{
    Task<IReadOnlyList<SupplierListItem>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class CreateSupplier
{
    private readonly ISupplierWriteRepository _supplierWriteRepository;

    public CreateSupplier(ISupplierWriteRepository supplierWriteRepository)
    {
        _supplierWriteRepository = supplierWriteRepository;
    }

    public async Task<SupplierDto> ExecuteAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var supplier = new Supplier(
            Guid.NewGuid(),
            request.Name,
            request.SupplierNumber,
            DateTime.UtcNow);

        await _supplierWriteRepository.AddAsync(supplier, cancellationToken);

        return new SupplierDto(
            supplier.Id,
            supplier.Name,
            supplier.SupplierNumber,
            supplier.IsActive,
            supplier.CreatedAtUtc);
    }
}
