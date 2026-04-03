using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Enums;

namespace SWFC.Domain.M200_Business.M204_Inventory.Entities;

public sealed class Stock
{
    private readonly List<StockMovement> _movements = new();
<<<<<<< HEAD
    private readonly List<StockReservation> _reservations = new();
=======
>>>>>>> origin/main

    private Stock()
    {
        Id = Guid.Empty;
        InventoryItemId = Guid.Empty;
        AuditInfo = null!;
    }

    private Stock(Guid id, Guid inventoryItemId, int quantityOnHand, AuditInfo auditInfo)
    {
        Id = id;
        InventoryItemId = inventoryItemId;
        QuantityOnHand = quantityOnHand;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public AuditInfo AuditInfo { get; private set; }
    public IReadOnlyCollection<StockMovement> Movements => _movements;

    public IReadOnlyCollection<StockMovement> Movements => _movements;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;

    public static Stock Create(
        Guid inventoryItemId,
        int quantityOnHand,
        ChangeContext changeContext)
    {
        if (inventoryItemId == Guid.Empty)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (quantityOnHand < 0)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Stock(Guid.NewGuid(), inventoryItemId, quantityOnHand, auditInfo);
    }

    public void SetQuantity(int quantityOnHand, ChangeContext changeContext)
    {
        if (quantityOnHand < 0)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        QuantityOnHand = quantityOnHand;
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }

    public void ApplyMovement(StockMovement movement, ChangeContext changeContext)
    {
        if (movement.StockId != Id)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        var nextQuantity = QuantityOnHand + movement.QuantityDelta;

        if (nextQuantity < 0)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        QuantityOnHand = nextQuantity;
        _movements.Add(movement);

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
<<<<<<< HEAD

    public int GetReservedQuantity()
    {
        return _reservations
            .Where(x => x.Status == StockReservationStatus.Active)
            .Sum(x => x.Quantity);
    }

    public int GetAvailableQuantity()
    {
        return QuantityOnHand - GetReservedQuantity();
    }

    public void AddReservation(StockReservation reservation, ChangeContext changeContext)
    {
        if (reservation.StockId != Id)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (reservation.Status != StockReservationStatus.Active)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (reservation.Quantity > GetAvailableQuantity())
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        _reservations.Add(reservation);

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
=======
>>>>>>> origin/main
}