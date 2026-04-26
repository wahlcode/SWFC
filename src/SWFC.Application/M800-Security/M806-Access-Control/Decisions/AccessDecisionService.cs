using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Application.M800_Security.M806_AccessControl.Decisions;

public sealed class AccessDecisionService : IAccessDecisionService
{
    private static readonly IReadOnlySet<AccessAction> ActionPermissionsRequired = new HashSet<AccessAction>
    {
        AccessAction.CanCreate,
        AccessAction.CanUpdate,
        AccessAction.CanDelete,
        AccessAction.CanApprove,
        AccessAction.CanExecute,
        AccessAction.CanControl,
        AccessAction.CanAdminister
    };

    public Task<AccessDecisionResult> DecideAsync(
        SecurityContext securityContext,
        AccessDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (securityContext.IsDeveloperMode)
        {
            return Task.FromResult(AccessDecisionResult.Allowed(request, "Developer mode override."));
        }

        if (!securityContext.IsAuthenticated)
        {
            return Task.FromResult(AccessDecisionResult.Denied(request, "User is not authenticated."));
        }

        if (RequiresModuleAccess(request.ModuleCode) &&
            !securityContext.HasModuleAccess(request.ModuleCode))
        {
            return Task.FromResult(AccessDecisionResult.Denied(
                request,
                $"Missing module access: {request.ModuleCode}."));
        }

        foreach (var role in request.RequiredRoles)
        {
            if (!securityContext.HasRole(role))
            {
                return Task.FromResult(AccessDecisionResult.Denied(
                    request,
                    $"Missing required role: {role}."));
            }
        }

        foreach (var permission in request.RequiredPermissions)
        {
            if (!securityContext.HasPermission(permission))
            {
                return Task.FromResult(AccessDecisionResult.Denied(
                    request,
                    $"Missing required permission: {permission}."));
            }
        }

        if (ActionPermissionsRequired.Contains(request.Action) &&
            request.RequiredRoles.Count == 0 &&
            request.RequiredPermissions.Count == 0)
        {
            return Task.FromResult(AccessDecisionResult.Denied(
                request,
                $"Action {request.Action} requires an explicit role or permission."));
        }

        if (request.AllowedOrganizationUnitIds.Count > 0 &&
            !request.AllowedOrganizationUnitIds.Any(
                organizationUnitId => securityContext.OrganizationUnitIds.Contains(
                    organizationUnitId,
                    StringComparer.OrdinalIgnoreCase)))
        {
            return Task.FromResult(AccessDecisionResult.Denied(
                request,
                "Organization context does not match."));
        }

        var attributes = BuildAttributeBag(securityContext);

        foreach (var attribute in request.DeniedAttributes)
        {
            if (attributes.TryGetValue(attribute.Key, out var actualValue) &&
                string.Equals(actualValue, attribute.Value, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AccessDecisionResult.Denied(
                    request,
                    $"Denied attribute matched: {attribute.Key}."));
            }
        }

        foreach (var attribute in request.RequiredAttributes)
        {
            if (!attributes.TryGetValue(attribute.Key, out var actualValue) ||
                !string.Equals(actualValue, attribute.Value, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AccessDecisionResult.Denied(
                    request,
                    $"Missing required attribute: {attribute.Key}."));
            }
        }

        if (request.RequiresApproval && string.IsNullOrWhiteSpace(request.ApprovedByUserId))
        {
            return Task.FromResult(AccessDecisionResult.NeedsApproval(
                request,
                $"Action {request.Action} requires approval."));
        }

        return Task.FromResult(AccessDecisionResult.Allowed(
            request,
            $"Action {request.Action} allowed."));
    }

    private static IReadOnlyDictionary<string, string> BuildAttributeBag(SecurityContext securityContext)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["user.id"] = securityContext.UserId,
            ["identity.key"] = securityContext.IdentityKey,
            ["user.name"] = securityContext.Username,
            ["culture"] = securityContext.PreferredCultureName,
            ["authenticated"] = securityContext.IsAuthenticated.ToString(),
            ["developerMode"] = securityContext.IsDeveloperMode.ToString()
        };

        foreach (var role in securityContext.Roles)
        {
            attributes[$"role.{role}"] = "true";
        }

        foreach (var permission in securityContext.Permissions)
        {
            attributes[$"permission.{permission}"] = "true";
        }

        foreach (var module in securityContext.PermissionModules)
        {
            attributes[$"module.{module}"] = "true";
        }

        return attributes;
    }

    private static bool RequiresModuleAccess(string moduleCode) =>
        !string.Equals(moduleCode, "System", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(moduleCode, "General", StringComparison.OrdinalIgnoreCase);
}
