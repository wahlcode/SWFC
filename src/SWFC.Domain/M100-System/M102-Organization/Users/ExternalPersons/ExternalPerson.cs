using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;

public sealed class ExternalPerson
{
    private ExternalPerson(
        Guid id,
        string displayName,
        string companyName,
        string? email,
        string? phone,
        string? function,
        Guid? organizationUnitId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        DisplayName = displayName;
        CompanyName = companyName;
        Email = email;
        Phone = phone;
        Function = function;
        OrganizationUnitId = organizationUnitId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    private ExternalPerson()
    {
    }

    public Guid Id { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Function { get; private set; }
    public Guid? OrganizationUnitId { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; } = null!;

    public static ExternalPerson Create(
        string displayName,
        string companyName,
        string? email,
        string? phone,
        string? function,
        Guid? organizationUnitId,
        ChangeContext changeContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentNullException.ThrowIfNull(changeContext);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new ExternalPerson(
            Guid.NewGuid(),
            displayName.Trim(),
            companyName.Trim(),
            Normalize(email),
            Normalize(phone),
            Normalize(function),
            organizationUnitId,
            true,
            auditInfo);
    }

    public void Update(
        string displayName,
        string companyName,
        string? email,
        string? phone,
        string? function,
        Guid? organizationUnitId,
        ChangeContext changeContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentNullException.ThrowIfNull(changeContext);

        DisplayName = displayName.Trim();
        CompanyName = companyName.Trim();
        Email = Normalize(email);
        Phone = Normalize(phone);
        Function = Normalize(function);
        OrganizationUnitId = organizationUnitId;

        Touch(changeContext);
    }

    public void Activate(ChangeContext changeContext)
    {
        ArgumentNullException.ThrowIfNull(changeContext);

        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(changeContext);
    }

    public void Deactivate(ChangeContext changeContext)
    {
        ArgumentNullException.ThrowIfNull(changeContext);

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

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}