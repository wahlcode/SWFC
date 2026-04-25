using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Users.Delegations;

public sealed class UserDelegation
{
    private UserDelegation(
        Guid id,
        Guid userId,
        Guid delegateUserId,
        string delegationType,
        DateTime validFromUtc,
        DateTime? validToUtc,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        UserId = userId;
        DelegateUserId = delegateUserId;
        DelegationType = delegationType;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    private UserDelegation()
    {
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid DelegateUserId { get; private set; }
    public string DelegationType { get; private set; } = string.Empty;
    public DateTime ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; } = null!;

    public static UserDelegation Create(
        Guid userId,
        Guid delegateUserId,
        string delegationType,
        DateTime validFromUtc,
        DateTime? validToUtc,
        ChangeContext changeContext)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        if (delegateUserId == Guid.Empty)
            throw new ArgumentException("Delegate user id is required.", nameof(delegateUserId));

        if (userId == delegateUserId)
            throw new ArgumentException("User and delegate user must be different.");

        ArgumentException.ThrowIfNullOrWhiteSpace(delegationType);
        ArgumentNullException.ThrowIfNull(changeContext);

        if (validToUtc.HasValue && validToUtc.Value < validFromUtc)
            throw new ArgumentException("ValidToUtc must be greater than or equal to ValidFromUtc.");

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new UserDelegation(
            Guid.NewGuid(),
            userId,
            delegateUserId,
            delegationType.Trim(),
            validFromUtc,
            validToUtc,
            true,
            auditInfo);
    }

    public void UpdateWindow(
        DateTime validFromUtc,
        DateTime? validToUtc,
        ChangeContext changeContext)
    {
        ArgumentNullException.ThrowIfNull(changeContext);

        if (validToUtc.HasValue && validToUtc.Value < validFromUtc)
            throw new ArgumentException("ValidToUtc must be greater than or equal to ValidFromUtc.");

        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;

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

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}