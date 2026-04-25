using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.CostCenters;

public sealed class CostCenter
{
    private CostCenter()
    {
        Id = Guid.Empty;
        Name = null!;
        Code = null!;
        ParentCostCenterId = null;
        IsActive = true;
        AuditInfo = null!;
    }

    private CostCenter(
        Guid id,
        CostCenterName name,
        CostCenterCode code,
        Guid? parentCostCenterId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Code = code;
        ParentCostCenterId = parentCostCenterId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public CostCenterName Name { get; private set; }
    public CostCenterCode Code { get; private set; }
    public Guid? ParentCostCenterId { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static CostCenter Create(
        CostCenterName name,
        CostCenterCode code,
        Guid? parentCostCenterId,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new CostCenter(
            Guid.NewGuid(),
            name,
            code,
            parentCostCenterId,
            isActive: true,
            auditInfo);
    }

    public void UpdateDetails(
        CostCenterName name,
        CostCenterCode code,
        Guid? parentCostCenterId,
        ChangeContext changeContext)
    {
        Name = name;
        Code = code;
        ParentCostCenterId = parentCostCenterId;

        Touch(changeContext);
    }

    public void Activate(ChangeContext changeContext)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(changeContext);
    }

    public void Deactivate(ChangeContext changeContext)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
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
}
