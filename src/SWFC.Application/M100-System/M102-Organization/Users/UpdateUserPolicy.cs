using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed class UpdateUserPolicy : IAuthorizationPolicy<UpdateUserCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateUserCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}
