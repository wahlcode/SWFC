using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M200_Business.M201_Assets.Services;

public sealed class DeleteMachinePolicy : IAuthorizationPolicy<DeleteMachineCommand>
{
    public AuthorizationRequirement GetRequirement(DeleteMachineCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.delete" });
    }
}