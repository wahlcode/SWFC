namespace SWFC.Application.M200_Business.M206_Purchasing.Suppliers;

public sealed class GetSuppliers
{
    private readonly ISupplierReadRepository _supplierReadRepository;

    public GetSuppliers(ISupplierReadRepository supplierReadRepository)
    {
        _supplierReadRepository = supplierReadRepository;
    }

    public Task<IReadOnlyList<SupplierListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _supplierReadRepository.GetAllAsync(cancellationToken);
    }
}
