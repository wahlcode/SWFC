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
using SWFC.Domain.M200_Business.M204_Inventory.Enums;


namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class ConsumeStockReservationHandler : IUseCaseHandler<ConsumeStockReservationCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockReservationWriteRepository _stockReservationWriteRepository;
    private readonly IStockMovementWriteRepository _stockMovementWriteRepository;
    private readonly IAuditService _auditService;

    public ConsumeStockReservationHandler(
        ICurrentUserService currentUserService,
        IStockReservationWriteRepository stockReservationWriteRepository,
        IStockMovementWriteRepository stockMovementWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _stockReservationWriteRepository = stockReservationWriteRepository;
        _stockMovementWriteRepository = stockMovementWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        ConsumeStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _stockReservationWriteRepository.GetByIdAsync(
            command.StockReservationId,
            cancellationToken);

        if (reservation is null)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"Stock reservation '{command.StockReservationId}' was not found.",
                ErrorCategory.NotFound));
        }

        var stock = await _stockReservationWriteRepository.GetStockByIdAsync(
            reservation.StockId,
            cancellationToken);

        if (stock is null)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"Stock '{reservation.StockId}' was not found.",
                ErrorCategory.NotFound));
        }

        if (reservation.Status != StockReservationStatus.Active)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.Validation.Invalid,
                "Only active reservations can be consumed.",
                ErrorCategory.Validation));
        }

        if (command.Quantity > reservation.Quantity)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.Validation.Invalid,
                "Consumption quantity exceeds reserved quantity.",
                ErrorCategory.Validation));
        }

        if (command.Quantity > stock.QuantityOnHand)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.Validation.Invalid,
                "Consumption quantity exceeds stock on hand.",
                ErrorCategory.Validation));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            ReservationId = reservation.Id,
            ReservationStockId = reservation.StockId,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            ReservationCreatedAtUtc = reservation.AuditInfo.CreatedAtUtc,
            ReservationCreatedBy = reservation.AuditInfo.CreatedBy,
            ReservationLastModifiedAtUtc = reservation.AuditInfo.LastModifiedAtUtc,
            ReservationLastModifiedBy = reservation.AuditInfo.LastModifiedBy,
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.QuantityOnHand,
            ReservedQuantity = stock.GetReservedQuantity(),
            AvailableQuantity = stock.GetAvailableQuantity()
        });

        reservation.Consume(command.Quantity, changeContext);

        var movement = StockMovement.Create(
            stock.Id,
            StockMovementType.GoodsIssue,
            -command.Quantity,
            changeContext);

        stock.ApplyMovement(movement, changeContext);

        await _stockMovementWriteRepository.AddAsync(movement, cancellationToken);

        var newValues = JsonSerializer.Serialize(new
        {
            ReservationId = reservation.Id,
            ReservationStockId = reservation.StockId,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            ReservationCreatedAtUtc = reservation.AuditInfo.CreatedAtUtc,
            ReservationCreatedBy = reservation.AuditInfo.CreatedBy,
            ReservationLastModifiedAtUtc = reservation.AuditInfo.LastModifiedAtUtc,
            ReservationLastModifiedBy = reservation.AuditInfo.LastModifiedBy,
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.QuantityOnHand,
            ReservedQuantity = stock.GetReservedQuantity(),
            AvailableQuantity = stock.GetAvailableQuantity(),
            MovementId = movement.Id,
            MovementStockId = movement.StockId,
            movement.MovementType,
            movement.QuantityDelta,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "ConsumeStockReservation",
            entity: "StockReservation",
            entityId: reservation.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _stockMovementWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movement.Id);
    }
}