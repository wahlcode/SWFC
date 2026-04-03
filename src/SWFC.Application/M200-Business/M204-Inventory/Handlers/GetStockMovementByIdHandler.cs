using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class GetStockMovementByIdHandler : IUseCaseHandler<GetStockMovementByIdQuery, StockMovementDetailsDto>
{
    private readonly IStockMovementReadRepository _stockMovementReadRepository;

    public GetStockMovementByIdHandler(IStockMovementReadRepository stockMovementReadRepository)
    {
        _stockMovementReadRepository = stockMovementReadRepository;
    }

    public async Task<Result<StockMovementDetailsDto>> HandleAsync(
        GetStockMovementByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var movement = await _stockMovementReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (movement is null)
        {
            return Result<StockMovementDetailsDto>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"StockMovement '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<StockMovementDetailsDto>.Success(movement);
    }
}