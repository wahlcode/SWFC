using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M100_System.M102_Organization.Authorization;

public sealed class GetOrganizationUnitByIdPolicy : IAuthorizationPolicy<GetOrganizationUnitByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetOrganizationUnitByIdQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}