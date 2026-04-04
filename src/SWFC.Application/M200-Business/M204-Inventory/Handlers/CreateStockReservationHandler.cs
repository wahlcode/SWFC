using System.Text.Json;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class CreateStockReservationHandler : IUseCaseHandler<CreateStockReservationCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockReservationWriteRepository _stockReservationWriteRepository;
    private readonly IAuditService _auditService;

    public CreateStockReservationHandler(
        ICurrentUserService currentUserService,
        IStockReservationWriteRepository stockReservationWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _stockReservationWriteRepository = stockReservationWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateStockReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var stock = await _stockReservationWriteRepository.GetStockByIdAsync(command.StockId, cancellationToken);

        if (stock is null)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"Stock '{command.StockId}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.QuantityOnHand,
            ReservedQuantity = stock.GetReservedQuantity(),
            AvailableQuantity = stock.GetAvailableQuantity()
        });

        StockReservation reservation;

        try
        {
            reservation = StockReservation.Create(
                command.StockId,
                command.Quantity,
                command.Note,
                command.TargetType,
                command.TargetReference,
                changeContext);

            stock.AddReservation(reservation, changeContext);
        }
        catch (ValidationException)
        {
            return Result<Guid>.Failure(new Error(
                ErrorCodes.Validation.Invalid,
                "Reservation quantity exceeds available stock or contains invalid data.",
                ErrorCategory.Validation));
        }

        await _stockReservationWriteRepository.AddAsync(reservation, cancellationToken);

        var newValues = JsonSerializer.Serialize(new
        {
            StockId = stock.Id,
            stock.InventoryItemId,
            stock.QuantityOnHand,
            ReservedQuantity = stock.GetReservedQuantity(),
            AvailableQuantity = stock.GetAvailableQuantity(),
            ReservationId = reservation.Id,
            reservation.Quantity,
            reservation.Note,
            reservation.Status,
            reservation.TargetType,
            reservation.TargetReference,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateStockReservation",
            entity: "StockReservation",
            entityId: reservation.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _stockReservationWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reservation.Id);
    }
}