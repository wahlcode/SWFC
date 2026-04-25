using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Domain.M200_Business.M204_Inventory.Stock;

public sealed class StockMovement
{
    private StockMovement()
    {
        Id = Guid.Empty;
        StockId = Guid.Empty;
        TargetReference = null;
        AuditInfo = null!;
    }

    private StockMovement(
        Guid id,
        Guid stockId,
        StockMovementType movementType,
        int quantityDelta,
        InventoryTargetType? targetType,
        string? targetReference,
        AuditInfo auditInfo)
    {
        Id = id;
        StockId = stockId;
        MovementType = movementType;
        QuantityDelta = quantityDelta;
        TargetType = targetType;
        TargetReference = targetReference;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid StockId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public int QuantityDelta { get; private set; }
    public AuditInfo AuditInfo { get; private set; }
    public InventoryTargetType? TargetType { get; private set; }
    public string? TargetReference { get; private set; }

    public static StockMovement Create(
        Guid stockId,
        StockMovementType movementType,
        int quantityDelta,
        ChangeContext changeContext,
        InventoryTargetType? targetType = null,
        string? targetReference = null)
    {
        if (stockId == Guid.Empty)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (!Enum.IsDefined(movementType))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (quantityDelta == 0)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (movementType == StockMovementType.GoodsReceipt && quantityDelta <= 0)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if ((movementType == StockMovementType.GoodsIssue || movementType == StockMovementType.Consumption) && quantityDelta >= 0)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (targetType is null && !string.IsNullOrWhiteSpace(targetReference))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (targetType is not null && string.IsNullOrWhiteSpace(targetReference))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new StockMovement(
            Guid.NewGuid(),
            stockId,
            movementType,
            quantityDelta,
            targetType,
            string.IsNullOrWhiteSpace(targetReference) ? null : targetReference.Trim(),
            auditInfo);
    }
}
