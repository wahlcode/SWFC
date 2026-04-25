using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;

namespace SWFC.Domain.M200_Business.M204_Inventory.Locations;

public sealed class Location
{
    private Location()
    {
        Id = Guid.Empty;
        Name = null!;
        Code = null!;
        AuditInfo = null!;
    }

    private Location(
        Guid id,
        LocationName name,
        LocationCode code,
        Guid? parentLocationId,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Code = code;
        ParentLocationId = parentLocationId;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public LocationName Name { get; private set; }
    public LocationCode Code { get; private set; }
    public Guid? ParentLocationId { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Location Create(
        LocationName name,
        LocationCode code,
        Guid? parentLocationId,
        ChangeContext changeContext)
    {
        if (parentLocationId.HasValue && parentLocationId.Value == Guid.Empty)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Location(
            Guid.NewGuid(),
            name,
            code,
            parentLocationId,
            auditInfo);
    }

    public void Update(
        LocationName name,
        LocationCode code,
        Guid? parentLocationId,
        ChangeContext changeContext)
    {
        if (parentLocationId.HasValue && parentLocationId.Value == Guid.Empty)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (parentLocationId == Id)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        Name = name;
        Code = code;
        ParentLocationId = parentLocationId;

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}

