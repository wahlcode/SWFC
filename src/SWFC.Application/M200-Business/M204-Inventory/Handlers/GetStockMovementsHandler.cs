using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class GetStockMovementsHandler : IUseCaseHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItem>>
{
    private readonly IStockMovementReadRepository _stockMovementReadRepository;

    public GetStockMovementsHandler(IStockMovementReadRepository stockMovementReadRepository)
    {
        _stockMovementReadRepository = stockMovementReadRepository;
    }

    public async Task<Result<IReadOnlyList<StockMovementListItem>>> HandleAsync(
        GetStockMovementsQuery command,
        CancellationToken cancellationToken = default)
    {
        var movements = await _stockMovementReadRepository.GetAllAsync(command.StockId, cancellationToken);
        return Result<IReadOnlyList<StockMovementListItem>>.Success(movements);
    }
}