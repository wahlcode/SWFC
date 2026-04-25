using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Assignments;

namespace SWFC.Domain.M100_System.M102_Organization.Users;

public sealed partial class User
{
    private readonly List<UserOrganizationUnit> _organizationUnits = new();

    private User()
    {
        Id = Guid.Empty;
        IdentityKey = null!;
        Username = null!;
        DisplayName = null!;
        FirstName = string.Empty;
        LastName = string.Empty;
        EmployeeNumber = string.Empty;
        BusinessEmail = string.Empty;
        BusinessPhone = string.Empty;
        Plant = string.Empty;
        Location = string.Empty;
        Team = string.Empty;
        CostCenterId = null;
        ShiftModelId = null;
        JobFunction = string.Empty;
        PreferredCultureName = "en-US";
        Status = UserStatus.Inactive;
        UserType = UserType.Internal;
        IsActive = false;
        AuditInfo = null!;
    }

    private User(
        Guid id,
        UserIdentityKey identityKey,
        Username username,
        UserDisplayName displayName,
        string firstName,
        string lastName,
        string employeeNumber,
        string businessEmail,
        string businessPhone,
        string plant,
        string location,
        string team,
        Guid? costCenterId,
        Guid? shiftModelId,
        string jobFunction,
        string preferredCultureName,
        UserStatus status,
        UserType userType,
        AuditInfo auditInfo)
    {
        Id = id;
        IdentityKey = identityKey;
        Username = username;
        DisplayName = displayName;
        FirstName = NormalizeRequiredValue(firstName, nameof(FirstName), 100);
        LastName = NormalizeRequiredValue(lastName, nameof(LastName), 100);
        EmployeeNumber = NormalizeRequiredValue(employeeNumber, nameof(EmployeeNumber), 100);
        BusinessEmail = NormalizeRequiredValue(businessEmail, nameof(BusinessEmail), 200);
        BusinessPhone = NormalizeRequiredValue(businessPhone, nameof(BusinessPhone), 50);
        Plant = NormalizeRequiredValue(plant, nameof(Plant), 100);
        Location = NormalizeRequiredValue(location, nameof(Location), 100);
        Team = NormalizeRequiredValue(team, nameof(Team), 100);
        CostCenterId = costCenterId;
        ShiftModelId = shiftModelId;
        JobFunction = NormalizeRequiredValue(jobFunction, nameof(JobFunction), 150);
        PreferredCultureName = NormalizeCultureName(preferredCultureName);
        Status = status;
        UserType = userType;
        IsActive = status == UserStatus.Active;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public UserIdentityKey IdentityKey { get; private set; }
    public Username Username { get; private set; }
    public UserDisplayName DisplayName { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string EmployeeNumber { get; private set; }
    public string BusinessEmail { get; private set; }
    public string BusinessPhone { get; private set; }
    public string Plant { get; private set; }
    public string Location { get; private set; }
    public string Team { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? ShiftModelId { get; private set; }
    public string JobFunction { get; private set; }
    public string PreferredCultureName { get; private set; }
    public UserStatus Status { get; private set; }
    public UserType UserType { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<UserOrganizationUnit> OrganizationUnits => _organizationUnits;

    public static User Create(
        UserIdentityKey identityKey,
        Username username,
        UserDisplayName displayName,
        string firstName,
        string lastName,
        string employeeNumber,
        string businessEmail,
        string businessPhone,
        string plant,
        string location,
        string team,
        Guid? costCenterId,
        Guid? shiftModelId,
        string jobFunction,
        string preferredCultureName,
        UserStatus status,
        UserType userType,
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
            firstName,
            lastName,
            employeeNumber,
            businessEmail,
            businessPhone,
            plant,
            location,
            team,
            costCenterId,
            shiftModelId,
            jobFunction,
            preferredCultureName,
            status,
            userType,
            auditInfo);
    }

    public void UpdateDetails(
        Username username,
        UserDisplayName displayName,
        string firstName,
        string lastName,
        string employeeNumber,
        string businessEmail,
        string businessPhone,
        string plant,
        string location,
        string team,
        Guid? costCenterId,
        Guid? shiftModelId,
        string jobFunction,
        string preferredCultureName,
        UserType userType,
        ChangeContext changeContext)
    {
        Username = username;
        DisplayName = displayName;
        FirstName = NormalizeRequiredValue(firstName, nameof(FirstName), 100);
        LastName = NormalizeRequiredValue(lastName, nameof(LastName), 100);
        EmployeeNumber = NormalizeRequiredValue(employeeNumber, nameof(EmployeeNumber), 100);
        BusinessEmail = NormalizeRequiredValue(businessEmail, nameof(BusinessEmail), 200);
        BusinessPhone = NormalizeRequiredValue(businessPhone, nameof(BusinessPhone), 50);
        Plant = NormalizeRequiredValue(plant, nameof(Plant), 100);
        Location = NormalizeRequiredValue(location, nameof(Location), 100);
        Team = NormalizeRequiredValue(team, nameof(Team), 100);
        CostCenterId = costCenterId;
        ShiftModelId = shiftModelId;
        JobFunction = NormalizeRequiredValue(jobFunction, nameof(JobFunction), 150);
        PreferredCultureName = NormalizeCultureName(preferredCultureName);
        UserType = userType;

        Touch(changeContext);
    }

    public void ChangeStatus(
        UserStatus status,
        ChangeContext changeContext)
    {
        Status = status;
        IsActive = status == UserStatus.Active;

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

    private static string NormalizeRequiredValue(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ValidationException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string NormalizeCultureName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "en-US";
        }

        var normalized = value.Trim();

        if (normalized.Length > 20)
        {
            throw new ValidationException("PreferredCultureName must not exceed 20 characters.");
        }

        return normalized;
    }
}
