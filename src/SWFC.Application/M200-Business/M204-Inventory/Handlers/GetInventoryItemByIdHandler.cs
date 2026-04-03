using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class GetInventoryItemByIdHandler : IUseCaseHandler<GetInventoryItemByIdQuery, InventoryItemDetailsDto>
{
    private readonly IInventoryItemReadRepository _inventoryItemReadRepository;

    public GetInventoryItemByIdHandler(IInventoryItemReadRepository inventoryItemReadRepository)
    {
        _inventoryItemReadRepository = inventoryItemReadRepository;
    }

    public async Task<Result<InventoryItemDetailsDto>> HandleAsync(
        GetInventoryItemByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _inventoryItemReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (inventoryItem is null)
        {
            return Result<InventoryItemDetailsDto>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"InventoryItem '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<InventoryItemDetailsDto>.Success(inventoryItem);
    }
}