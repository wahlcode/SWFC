using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Enums;

namespace SWFC.Domain.M200_Business.M204_Inventory.Entities;

public sealed class StockReservation
{
    private StockReservation()
    {
        Id = Guid.Empty;
        StockId = Guid.Empty;
        Note = string.Empty;
        TargetReference = null;
        AuditInfo = null!;
    }

    private StockReservation(
        Guid id,
        Guid stockId,
        int quantity,
        string note,
        StockReservationStatus status,
        InventoryTargetType? targetType,
        string? targetReference,
        AuditInfo auditInfo)
    {
        Id = id;
        StockId = stockId;
        Quantity = quantity;
        Note = note;
        Status = status;
        TargetType = targetType;
        TargetReference = targetReference;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid StockId { get; private set; }
    public int Quantity { get; private set; }
    public string Note { get; private set; }
    public StockReservationStatus Status { get; private set; }
    public AuditInfo AuditInfo { get; private set; }
    public InventoryTargetType? TargetType { get; private set; }
    public string? TargetReference { get; private set; }

    public static StockReservation Create(
        Guid stockId,
        int quantity,
        string? note,
        InventoryTargetType? targetType,
        string? targetReference,
        ChangeContext changeContext)
    {
        if (stockId == Guid.Empty)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (quantity <= 0)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (targetType is null && !string.IsNullOrWhiteSpace(targetReference))
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (targetType is not null && string.IsNullOrWhiteSpace(targetReference))
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new StockReservation(
            Guid.NewGuid(),
            stockId,
            quantity,
            note?.Trim() ?? string.Empty,
            StockReservationStatus.Active,
            targetType,
            string.IsNullOrWhiteSpace(targetReference) ? null : targetReference.Trim(),
            auditInfo);
    }

    public void Release(ChangeContext changeContext)
    {
        if (Status == StockReservationStatus.Released)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        Status = StockReservationStatus.Released;

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }

    public void Consume(int quantity, ChangeContext changeContext)
    {
        if (Status != StockReservationStatus.Active)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (quantity <= 0)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        if (quantity > Quantity)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        Quantity -= quantity;

        if (Quantity == 0)
        {
            Status = StockReservationStatus.Released;
        }

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}