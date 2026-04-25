namespace SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;

public sealed class GetRequestForQuotations
{
    private readonly IRequestForQuotationReadRepository _requestForQuotationReadRepository;

    public GetRequestForQuotations(IRequestForQuotationReadRepository requestForQuotationReadRepository)
    {
        _requestForQuotationReadRepository = requestForQuotationReadRepository;
    }

    public Task<IReadOnlyList<RequestForQuotationListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _requestForQuotationReadRepository.GetAllAsync(cancellationToken);
    }
}
