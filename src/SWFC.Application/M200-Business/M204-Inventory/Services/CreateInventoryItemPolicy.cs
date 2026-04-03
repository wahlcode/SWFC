using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M204_Inventory.Services;

public sealed class CreateInventoryItemPolicy : IAuthorizationPolicy<CreateInventoryItemCommand>
{
    public AuthorizationRequirement GetRequirement(CreateInventoryItemCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.create" });
    }
}