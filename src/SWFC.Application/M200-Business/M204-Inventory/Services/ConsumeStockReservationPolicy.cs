using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M204_Inventory.Services;

public sealed class ConsumeStockReservationPolicy : IAuthorizationPolicy<ConsumeStockReservationCommand>
{
    public AuthorizationRequirement GetRequirement(ConsumeStockReservationCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "stock.write" });
    }
}