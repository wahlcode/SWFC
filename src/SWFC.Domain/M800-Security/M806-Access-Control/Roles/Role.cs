using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M800_Security.M806_AccessControl.Roles;

public sealed class Role
{
    private Role()
    {
        Id = Guid.Empty;
        Name = null!;
        Description = null;
        IsActive = true;
        IsSystemRole = false;
        AuditInfo = null!;
    }

    private Role(
        Guid id,
        RoleName name,
        string? description,
        bool isActive,
        bool isSystemRole,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        IsSystemRole = isSystemRole;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public RoleName Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSystemRole { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Role Create(
        RoleName name,
        string? description,
        ChangeContext changeContext,
        bool isSystemRole = false)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Role(
            Guid.NewGuid(),
            name,
            NormalizeDescription(description),
            isActive: true,
            isSystemRole: isSystemRole,
            auditInfo);
    }

    public void UpdateDetails(
        RoleName name,
        string? description,
        ChangeContext changeContext)
    {
        var normalizedDescription = NormalizeDescription(description);

        if (IsSystemRole &&
            (!string.Equals(Name.Value, name.Value, StringComparison.Ordinal) ||
             !string.Equals(Description, normalizedDescription, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("System role metadata is immutable.");
        }

        if (string.Equals(Name.Value, name.Value, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDescription, StringComparison.Ordinal))
        {
            return;
        }

        Name = name;
        Description = normalizedDescription;
        Touch(changeContext);
    }

    public void SynchronizeSystemDefinition(
        RoleName name,
        string? description,
        ChangeContext changeContext)
    {
        if (!IsSystemRole)
        {
            throw new InvalidOperationException("Only system roles can synchronize their system definition.");
        }

        var normalizedDescription = NormalizeDescription(description);

        if (string.Equals(Name.Value, name.Value, StringComparison.Ordinal) &&
            string.Equals(Description, normalizedDescription, StringComparison.Ordinal))
        {
            return;
        }

        Name = name;
        Description = normalizedDescription;
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
        if (IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be deactivated.");
        }

        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(changeContext);
    }

    public void MarkAsSystemRole(ChangeContext changeContext)
    {
        if (IsSystemRole)
        {
            return;
        }

        IsSystemRole = true;
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
