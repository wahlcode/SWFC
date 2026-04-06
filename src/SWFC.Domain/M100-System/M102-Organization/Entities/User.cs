using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Entities;

public sealed class User
{
    private readonly List<UserRole> _roles = new();
    private readonly List<UserOrganizationUnit> _organizationUnits = new();

    private User()
    {
        Id = Guid.Empty;
        IdentityKey = null!;
        Username = null!;
        DisplayName = null!;
        IsActive = false;
        AuditInfo = null!;
    }

    private User(
        Guid id,
        UserIdentityKey identityKey,
        Username username,
        UserDisplayName displayName,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        IdentityKey = identityKey;
        Username = username;
        DisplayName = displayName;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public UserIdentityKey IdentityKey { get; private set; }
    public Username Username { get; private set; }
    public UserDisplayName DisplayName { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;
    public IReadOnlyCollection<UserOrganizationUnit> OrganizationUnits => _organizationUnits;

    public static User Create(
        UserIdentityKey identityKey,
        Username username,
        UserDisplayName displayName,
        bool isActive,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new User(
            Guid.NewGuid(),
            identityKey,
            username,
            displayName,
            isActive,
            auditInfo);
    }

    public void UpdateDetails(
        Username username,
        UserDisplayName displayName,
        bool isActive,
        ChangeContext changeContext)
    {
        Username = username;
        DisplayName = displayName;
        IsActive = isActive;

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }

    public bool HasRole(Guid roleId) => _roles.Any(x => x.RoleId == roleId);

    public bool HasOrganizationUnit(Guid organizationUnitId) =>
        _organizationUnits.Any(x => x.OrganizationUnitId == organizationUnitId);

    public void AssignRole(Guid roleId, ChangeContext changeContext)
    {
        if (HasRole(roleId))
        {
            return;
        }

        _roles.Add(UserRole.Create(Id, roleId, changeContext));
    }

    public void RemoveRole(Guid roleId)
    {
        var existing = _roles.FirstOrDefault(x => x.RoleId == roleId);

        if (existing is null)
        {
            return;
        }

        _roles.Remove(existing);
    }

    public void AssignOrganizationUnit(
        Guid organizationUnitId,
        bool isPrimary,
        ChangeContext changeContext)
    {
        if (HasOrganizationUnit(organizationUnitId))
        {
            if (isPrimary)
            {
                foreach (var item in _organizationUnits)
                {
                    item.SetPrimary(item.OrganizationUnitId == organizationUnitId, changeContext);
                }
            }

            return;
        }

        if (isPrimary)
        {
            foreach (var item in _organizationUnits)
            {
                item.SetPrimary(false, changeContext);
            }
        }

        _organizationUnits.Add(
            UserOrganizationUnit.Create(
                Id,
                organizationUnitId,
                isPrimary,
                changeContext));
    }

    public void RemoveOrganizationUnit(Guid organizationUnitId)
    {
        var existing = _organizationUnits.FirstOrDefault(x => x.OrganizationUnitId == organizationUnitId);

        if (existing is null)
        {
            return;
        }

        _organizationUnits.Remove(existing);
    }
}