using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M204_Inventory.Services;

public sealed class GetStockReservationsPolicy : IAuthorizationPolicy<GetStockReservationsQuery>
{
    public AuthorizationRequirement GetRequirement(GetStockReservationsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockreservation.read" });
    }
}