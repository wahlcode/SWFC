using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M800_Security.M806_AccessControl.Permissions;

public sealed class Permission
{
    private Permission()
    {
        Id = Guid.Empty;
        Code = null!;
        Name = null!;
        Description = null;
        Module = null!;
        IsActive = true;
        AuditInfo = null!;
    }

    private Permission(
        Guid id,
        string code,
        string name,
        string? description,
        string module,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        Module = module;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Module { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Permission Create(
        string code,
        string name,
        string? description,
        string module,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Permission(
            Guid.NewGuid(),
            code.Trim(),
            name.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            module.Trim(),
            true,
            auditInfo);
    }

    public void UpdateDetails(
        string name,
        string? description,
        string module,
        ChangeContext changeContext)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
        Module = module.Trim();

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
