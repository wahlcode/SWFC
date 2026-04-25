using SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;

namespace SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;

public interface IRequestForQuotationWriteRepository
{
    Task AddAsync(RequestForQuotation requestForQuotation, CancellationToken cancellationToken = default);
}

public interface IRequestForQuotationReadRepository
{
    Task<IReadOnlyList<RequestForQuotationListItem>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class CreateRequestForQuotation
{
    private readonly IRequestForQuotationWriteRepository _requestForQuotationWriteRepository;

    public CreateRequestForQuotation(IRequestForQuotationWriteRepository requestForQuotationWriteRepository)
    {
        _requestForQuotationWriteRepository = requestForQuotationWriteRepository;
    }

    public async Task<RequestForQuotationDto> ExecuteAsync(
        CreateRequestForQuotationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestForQuotation = new RequestForQuotation(
            Guid.NewGuid(),
            request.PurchaseRequirementId,
            request.SupplierId,
            DateTime.UtcNow,
            request.ResponseDueAtUtc);

        await _requestForQuotationWriteRepository.AddAsync(requestForQuotation, cancellationToken);

        return new RequestForQuotationDto(
            requestForQuotation.Id,
            requestForQuotation.PurchaseRequirementId,
            requestForQuotation.SupplierId,
            requestForQuotation.RequestedAtUtc,
            requestForQuotation.ResponseDueAtUtc,
            requestForQuotation.IsClosed);
    }
}
