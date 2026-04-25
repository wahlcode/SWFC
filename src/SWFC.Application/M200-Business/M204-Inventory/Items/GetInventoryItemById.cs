using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record GetInventoryItemByIdQuery(Guid Id);

public sealed class GetInventoryItemByIdValidator : ICommandValidator<GetInventoryItemByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetInventoryItemByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Inventory item id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetInventoryItemByIdPolicy : IAuthorizationPolicy<GetInventoryItemByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetInventoryItemByIdQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.read" });
    }
}

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
                GeneralErrorCodes.NotFound,
                $"InventoryItem '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<InventoryItemDetailsDto>.Success(inventoryItem);
    }
}

