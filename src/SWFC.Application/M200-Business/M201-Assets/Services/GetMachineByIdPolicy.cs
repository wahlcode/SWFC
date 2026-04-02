using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M201_Assets.Services;

public sealed class GetMachineByIdPolicy : IAuthorizationPolicy<GetMachineByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetMachineByIdQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.read" });
    }
}