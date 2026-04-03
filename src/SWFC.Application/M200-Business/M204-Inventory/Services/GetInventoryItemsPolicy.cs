using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M204_Inventory.Services;

public sealed class GetInventoryItemsPolicy : IAuthorizationPolicy<GetInventoryItemsQuery>
{
    public AuthorizationRequirement GetRequirement(GetInventoryItemsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.read" });
    }
}