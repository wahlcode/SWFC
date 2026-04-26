namespace SWFC.Domain.M200_Business.M210_Customers;

public enum CustomerAssignmentType
{
    Asset,
    Project,
    QualityCase,
    Document
}

public sealed record CustomerContact(string Name, string Role, string Email)
{
    public bool IsUsable => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Role);
}

public sealed record CustomerAssignment(CustomerAssignmentType Type, Guid? ObjectId, string Reference)
{
    public bool HasOperationalReference => !string.IsNullOrWhiteSpace(Reference) || ObjectId.HasValue;
}

public sealed class Customer
{
    private readonly List<CustomerContact> _contacts = new();
    private readonly List<CustomerAssignment> _assignments = new();

    public Customer(Guid id, string name, string erpReference)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = RequireText(name, nameof(name));
        ErpReference = RequireText(erpReference, nameof(erpReference));
        IsActive = true;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string ErpReference { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<CustomerContact> Contacts => _contacts;
    public IReadOnlyList<CustomerAssignment> Assignments => _assignments;

    public void AddContact(CustomerContact contact)
    {
        if (!contact.IsUsable)
        {
            throw new ArgumentException("Customer contact needs a name and role.", nameof(contact));
        }

        _contacts.Add(contact);
    }

    public void Assign(CustomerAssignment assignment)
    {
        if (!assignment.HasOperationalReference)
        {
            throw new ArgumentException("Customer assignment needs an object or reference.", nameof(assignment));
        }

        _assignments.Add(assignment);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
