using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M100_System.M102_Organization.Authorization;

public sealed class CreateRolePolicy : IAuthorizationPolicy<CreateRoleCommand>
{
    public AuthorizationRequirement GetRequirement(CreateRoleCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}