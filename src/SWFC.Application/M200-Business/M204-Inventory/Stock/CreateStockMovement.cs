using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;
using System.Text.Json;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed record CreateStockMovementCommand(
    Guid InventoryItemId,
    Guid LocationId,
    string? Bin,
    StockMovementType MovementType,
    int QuantityDelta,
    InventoryTargetType? TargetType,
    string? TargetReference,
    string Reason);

public sealed class CreateStockMovementValidator : ICommandValidator<CreateStockMovementCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateStockMovementCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (command.InventoryItemId == Guid.Empty)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Inventory item is required."));
        }

        if (command.LocationId == Guid.Empty)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Location is required."));
        }

        if (!string.IsNullOrWhiteSpace(command.Bin) && command.Bin.Trim().Length > 100)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Bin must not exceed 100 characters."));
        }

        if (!Enum.IsDefined(command.MovementType))
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Movement type is invalid."));
        }

        if (command.QuantityDelta == 0)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Quantity delta must not be zero."));
        }

        if (command.MovementType == StockMovementType.GoodsReceipt && command.QuantityDelta <= 0)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "GoodsReceipt requires a positive quantity delta."));
        }

        if (command.MovementType == StockMovementType.GoodsIssue && command.QuantityDelta >= 0)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "GoodsIssue requires a negative quantity delta."));
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Reason is required."));
        }

        if (!string.IsNullOrWhiteSpace(command.TargetReference) && command.TargetReference.Trim().Length > 200)
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Target reference must not exceed 200 characters."));
        }

        if (command.TargetType is null && !string.IsNullOrWhiteSpace(command.TargetReference))
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Target reference requires target type."));
        }

        if (command.TargetType is not null && string.IsNullOrWhiteSpace(command.TargetReference))
        {
            errors.Add(new ValidationError(
                ValidationErrorCodes.Invalid,
                "Target type requires target reference."));
        }

        return Task.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors.ToArray()));
    }
}

public sealed class CreateStockMovementPolicy : IAuthorizationPolicy<CreateStockMovementCommand>
{
    public AuthorizationRequirement GetRequirement(CreateStockMovementCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockmovement.create" });
    }
}

public sealed class CreateStockMovementHandler : IUseCaseHandler<CreateStockMovementCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockMovementWriteRepository _stockMovementWriteRepository;
    private readonly IAuditService _auditService;

    public CreateStockMovementHandler(
        ICurrentUserService currentUserService,
        IStockMovementWriteRepository stockMovementWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _stockMovementWriteRepository = stockMovementWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateStockMovementCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var stock = await _stockMovementWriteRepository.GetStockByInventoryItemAndLocationAsync(
            command.InventoryItemId,
            command.LocationId,
            command.Bin,
            cancellationToken);

        if (stock is null)
        {
            stock = StockEntity.Create(
                command.InventoryItemId,
                command.LocationId,
                command.Bin,
                0,
                changeContext);

            await _stockMovementWriteRepository.AddStockAsync(stock, cancellationToken);
        }

        var movement = StockMovement.Create(
            stock.Id,
            command.MovementType,
            command.QuantityDelta,
            changeContext,
            command.TargetType,
            command.TargetReference);

        var oldValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.LocationId,
            stock.Bin,
            stock.QuantityOnHand,
            stock.AuditInfo.CreatedAtUtc,
            stock.AuditInfo.CreatedBy,
            stock.AuditInfo.LastModifiedAtUtc,
            stock.AuditInfo.LastModifiedBy
        });

        stock.ApplyMovement(movement, changeContext);
        await _stockMovementWriteRepository.AddAsync(movement, cancellationToken);

        var newValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.LocationId,
            stock.Bin,
            stock.QuantityOnHand,
            MovementId = movement.Id,
            MovementStockId = movement.StockId,
            movement.MovementType,
            movement.QuantityDelta,
            movement.TargetType,
            movement.TargetReference,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateStockMovement",
            entity: "StockMovement",
            entityId: movement.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _stockMovementWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movement.Id);
    }
}

