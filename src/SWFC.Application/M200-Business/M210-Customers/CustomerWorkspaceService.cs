using SWFC.Domain.M200_Business.M210_Customers;

namespace SWFC.Application.M200_Business.M210_Customers;

public sealed record CustomerSummaryDto(
    Guid Id,
    string Name,
    string ErpReference,
    bool IsActive,
    int ContactCount,
    int AssignmentCount);

public sealed class CustomerWorkspaceService
{
    private readonly List<Customer> _customers = new();

    public CustomerWorkspaceService()
    {
        var customer = new Customer(
            Guid.Parse("21000000-0000-0000-0000-000000000001"),
            "Musterkunde Werk Nord",
            "SAP-C10042");
        customer.AddContact(new CustomerContact("Operations Ansprechpartner", "Betrieb", "operations@example.local"));
        customer.Assign(new CustomerAssignment(CustomerAssignmentType.Asset, Guid.Parse("20100000-0000-0000-0000-000000000001"), "Maschine A-01"));
        customer.Assign(new CustomerAssignment(CustomerAssignmentType.Project, Guid.Parse("20900000-0000-0000-0000-000000000001"), "KVP Energie und Wartung"));
        customer.Assign(new CustomerAssignment(CustomerAssignmentType.QualityCase, Guid.Parse("20700000-0000-0000-0000-000000000001"), "Reklamation Q-2026-01"));
        _customers.Add(customer);
    }

    public IReadOnlyList<CustomerSummaryDto> GetCustomers()
    {
        return _customers
            .Select(customer => new CustomerSummaryDto(
                customer.Id,
                customer.Name,
                customer.ErpReference,
                customer.IsActive,
                customer.Contacts.Count,
                customer.Assignments.Count))
            .ToList();
    }

    public Customer RegisterCustomer(string name, string erpReference)
    {
        var customer = new Customer(Guid.NewGuid(), name, erpReference);
        _customers.Add(customer);
        return customer;
    }
}
