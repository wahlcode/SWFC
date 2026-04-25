using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.ShiftModels;

public sealed class ShiftModel
{
    private ShiftModel()
    {
        Id = Guid.Empty;
        Name = null!;
        Code = null!;
        Description = null;
        IsActive = true;
        AuditInfo = null!;
    }

    private ShiftModel(
        Guid id,
        ShiftModelName name,
        ShiftModelCode code,
        string? description,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public ShiftModelName Name { get; private set; }
    public ShiftModelCode Code { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static ShiftModel Create(
        ShiftModelName name,
        ShiftModelCode code,
        string? description,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new ShiftModel(
            Guid.NewGuid(),
            name,
            code,
            NormalizeDescription(description),
            isActive: true,
            auditInfo);
    }

    public void UpdateDetails(
        ShiftModelName name,
        ShiftModelCode code,
        string? description,
        ChangeContext changeContext)
    {
        Name = name;
        Code = code;
        Description = NormalizeDescription(description);

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

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
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
