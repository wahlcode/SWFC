using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Domain.M200_Business.M204_Inventory.Stock;

public sealed class Stock
{
    private readonly List<StockMovement> _movements = new();
    private readonly List<StockReservation> _reservations = new();

    private Stock()
    {
        Id = Guid.Empty;
        InventoryItemId = Guid.Empty;
        LocationId = Guid.Empty;
        AuditInfo = null!;
    }

    private Stock(
        Guid id,
        Guid inventoryItemId,
        Guid locationId,
        string? bin,
        int quantityOnHand,
        AuditInfo auditInfo)
    {
        Id = id;
        InventoryItemId = inventoryItemId;
        LocationId = locationId;
        Bin = bin;
        QuantityOnHand = quantityOnHand;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid LocationId { get; private set; }
    public string? Bin { get; private set; }
    public int QuantityOnHand { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<StockMovement> Movements => _movements;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;

    public static Stock Create(
        Guid inventoryItemId,
        Guid locationId,
        string? bin,
        int quantityOnHand,
        ChangeContext changeContext)
    {
        if (inventoryItemId == Guid.Empty)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (locationId == Guid.Empty)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (quantityOnHand < 0)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Stock(
            Guid.NewGuid(),
            inventoryItemId,
            locationId,
            NormalizeBin(bin),
            quantityOnHand,
            auditInfo);
    }

    public void Relocate(Guid locationId, string? bin, ChangeContext changeContext)
    {
        if (locationId == Guid.Empty)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        LocationId = locationId;
        Bin = NormalizeBin(bin);

        Touch(changeContext);
    }

    public void ApplyInventoryCorrection(int correctedQuantityOnHand, ChangeContext changeContext)
    {
        if (correctedQuantityOnHand < 0)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        QuantityOnHand = correctedQuantityOnHand;
        Touch(changeContext);
    }

    public void ApplyMovement(StockMovement movement, ChangeContext changeContext)
    {
        if (movement.StockId != Id)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var nextQuantity = QuantityOnHand + movement.QuantityDelta;

        if (nextQuantity < 0)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        QuantityOnHand = nextQuantity;
        _movements.Add(movement);

        Touch(changeContext);
    }

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
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (reservation.Status != StockReservationStatus.Active)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (reservation.Quantity > GetAvailableQuantity())
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        _reservations.Add(reservation);

        Touch(changeContext);
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }

    private static string? NormalizeBin(string? bin)
    {
        if (string.IsNullOrWhiteSpace(bin))
        {
            return null;
        }

        var normalized = bin.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return normalized;
    }
}

