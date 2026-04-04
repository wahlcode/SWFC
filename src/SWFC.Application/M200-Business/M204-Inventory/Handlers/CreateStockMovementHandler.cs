using System.Text.Json;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

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

        var stock = await _stockMovementWriteRepository.GetStockByInventoryItemIdAsync(command.StockId, cancellationToken);

        if (stock is null)
        {
            stock = Stock.Create(
                command.StockId,
                0,
                changeContext);

            await _stockMovementWriteRepository.AddStockAsync(stock, cancellationToken);
        }

        var movement = StockMovement.Create(
            stock.Id,
            command.MovementType,
            command.QuantityDelta,
            changeContext);

        var oldValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
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
            stock.QuantityOnHand,
            MovementId = movement.Id,
            MovementStockId = movement.StockId,
            movement.MovementType,
            movement.QuantityDelta,
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