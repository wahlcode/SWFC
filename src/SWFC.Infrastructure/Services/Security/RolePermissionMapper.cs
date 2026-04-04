using SWFC.Application.M100_System.M102_Organization.Interfaces;

namespace SWFC.Infrastructure.Services.Security;

public sealed class RolePermissionMapper : IRolePermissionMapper
{
    public IReadOnlyCollection<string> Map(IEnumerable<string> roles)
    {
        var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                resolved.Add("machine.read");
                resolved.Add("machine.create");
                resolved.Add("machine.update");
                resolved.Add("machine.delete");

                resolved.Add("inventoryitem.read");
                resolved.Add("inventoryitem.create");
                resolved.Add("inventoryitem.update");
                resolved.Add("inventoryitem.delete");

                resolved.Add("stock.read");
                resolved.Add("stock.write");

                resolved.Add("stockmovement.read");
                resolved.Add("stockmovement.create");

                resolved.Add("stockreservation.read");
                resolved.Add("stockreservation.create");
                resolved.Add("stockreservation.release");

                resolved.Add("organization.read");
                resolved.Add("organization.write");
            }

            if (string.Equals(role, "Viewer", StringComparison.OrdinalIgnoreCase))
            {
                resolved.Add("machine.read");
                resolved.Add("inventoryitem.read");
                resolved.Add("stock.read");
                resolved.Add("stockmovement.read");
                resolved.Add("stockreservation.read");
                resolved.Add("organization.read");
            }

            if (string.Equals(role, "InventoryUser", StringComparison.OrdinalIgnoreCase))
            {
                resolved.Add("inventoryitem.read");
                resolved.Add("inventoryitem.create");
                resolved.Add("inventoryitem.update");
                resolved.Add("inventoryitem.delete");

                resolved.Add("stock.read");
                resolved.Add("stock.write");

                resolved.Add("stockmovement.read");
                resolved.Add("stockmovement.create");

                resolved.Add("stockreservation.read");
                resolved.Add("stockreservation.create");
                resolved.Add("stockreservation.release");
            }

            if (string.Equals(role, "MaintenanceUser", StringComparison.OrdinalIgnoreCase))
            {
                resolved.Add("machine.read");
                resolved.Add("machine.create");
                resolved.Add("machine.update");
            }
        }

        return resolved.ToArray();
    }
}