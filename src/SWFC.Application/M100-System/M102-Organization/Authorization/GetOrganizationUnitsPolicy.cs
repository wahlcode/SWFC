using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M100_System.M102_Organization.Authorization;

public sealed class GetOrganizationUnitsPolicy : IAuthorizationPolicy<GetOrganizationUnitsQuery>
{
    public AuthorizationRequirement GetRequirement(GetOrganizationUnitsQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}