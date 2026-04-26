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
    string Reason,
    Guid? TransferLocationId = null,
    string? TransferBin = null);

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

        var sourceBin = NormalizeBin(command.Bin);
        var stock = await _stockMovementWriteRepository.GetStockByInventoryItemAndLocationForUpdateAsync(
            command.InventoryItemId,
            command.LocationId,
            sourceBin,
            cancellationToken);

        if (stock is null)
        {
            if (command.MovementType is StockMovementType.GoodsIssue
                or StockMovementType.Transfer
                or StockMovementType.Consumption)
            {
                return Result<Guid>.Failure(new Error(
                    GeneralErrorCodes.NotFound,
                    "Source stock was not found.",
                    ErrorCategory.NotFound));
            }

            stock = StockEntity.Create(
                command.InventoryItemId,
                command.LocationId,
                sourceBin,
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

        StockMovement? transferTargetMovement = null;
        StockEntity? transferTargetStock = null;

        if (command.MovementType == StockMovementType.Transfer)
        {
            var targetLocationId = command.TransferLocationId!.Value;
            var targetBin = NormalizeBin(command.TransferBin);

            transferTargetStock = await _stockMovementWriteRepository.GetStockByInventoryItemAndLocationForUpdateAsync(
                command.InventoryItemId,
                targetLocationId,
                targetBin,
                cancellationToken);

            if (transferTargetStock is null)
            {
                transferTargetStock = StockEntity.Create(
                    command.InventoryItemId,
                    targetLocationId,
                    targetBin,
                    0,
                    changeContext);

                await _stockMovementWriteRepository.AddStockAsync(transferTargetStock, cancellationToken);
            }

            transferTargetMovement = StockMovement.Create(
                transferTargetStock.Id,
                StockMovementType.Transfer,
                Math.Abs(command.QuantityDelta),
                changeContext,
                command.TargetType,
                command.TargetReference);

            transferTargetStock.ApplyMovement(transferTargetMovement, changeContext);
            await _stockMovementWriteRepository.AddAsync(transferTargetMovement, cancellationToken);
        }

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
            TransferTargetStockId = transferTargetStock?.Id,
            TransferTargetMovementId = transferTargetMovement?.Id,
            TransferTargetLocationId = command.TransferLocationId,
            TransferTargetBin = NormalizeBin(command.TransferBin),
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

    private static string? NormalizeBin(string? bin)
    {
        return string.IsNullOrWhiteSpace(bin) ? null : bin.Trim();
    }
}

