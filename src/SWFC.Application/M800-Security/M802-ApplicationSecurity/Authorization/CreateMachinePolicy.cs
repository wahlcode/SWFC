using SWFC.Application.M200_Business.M201_Assets.Commands;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public sealed class CreateMachinePolicy : IAuthorizationPolicy<CreateMachineCommand>
{
    public AuthorizationRequirement GetRequirement(CreateMachineCommand request)
    {
        return new AuthorizationRequirement(
            requiredRoles: Array.Empty<string>(),
            requiredPermissions: new[] { "machine.create" });
    }
}