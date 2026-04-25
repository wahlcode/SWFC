using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed record GetStockMovementByIdQuery(Guid Id);

public sealed class GetStockMovementByIdValidator : ICommandValidator<GetStockMovementByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetStockMovementByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Stock movement id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetStockMovementByIdPolicy : IAuthorizationPolicy<GetStockMovementByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockMovementByIdQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockmovement.read" });
    }
}

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
                GeneralErrorCodes.NotFound,
                $"StockMovement '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<StockMovementDetailsDto>.Success(movement);
    }
}

