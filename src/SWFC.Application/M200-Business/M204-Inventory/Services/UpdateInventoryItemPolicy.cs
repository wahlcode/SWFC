using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M204_Inventory.Services;

public sealed class UpdateInventoryItemPolicy : IAuthorizationPolicy<UpdateInventoryItemCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateInventoryItemCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.update" });
    }
}