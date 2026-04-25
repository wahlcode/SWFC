namespace SWFC.Application.M800_Security.M806_AccessControl.Decisions;

public sealed class AccessDecisionRequest
{
    public AccessDecisionRequest(
        AccessAction action,
        string moduleCode,
        string objectType,
        string objectId,
        IReadOnlyCollection<string>? requiredRoles = null,
        IReadOnlyCollection<string>? requiredPermissions = null,
        IReadOnlyDictionary<string, string>? requiredAttributes = null,
        IReadOnlyDictionary<string, string>? deniedAttributes = null,
        IReadOnlyCollection<string>? allowedOrganizationUnitIds = null,
        bool requiresApproval = false,
        string? approvedByUserId = null)
    {
        Action = action;
        ModuleCode = Normalize(moduleCode);
        ObjectType = Normalize(objectType);
        ObjectId = Normalize(objectId);
        RequiredRoles = NormalizeCollection(requiredRoles);
        RequiredPermissions = NormalizeCollection(requiredPermissions);
        RequiredAttributes = NormalizeDictionary(requiredAttributes);
        DeniedAttributes = NormalizeDictionary(deniedAttributes);
        AllowedOrganizationUnitIds = NormalizeCollection(allowedOrganizationUnitIds);
        RequiresApproval = requiresApproval;
        ApprovedByUserId = string.IsNullOrWhiteSpace(approvedByUserId) ? null : approvedByUserId.Trim();
    }

    public AccessAction Action { get; }
    public string ModuleCode { get; }
    public string ObjectType { get; }
    public string ObjectId { get; }
    public IReadOnlyCollection<string> RequiredRoles { get; }
    public IReadOnlyCollection<string> RequiredPermissions { get; }
    public IReadOnlyDictionary<string, string> RequiredAttributes { get; }
    public IReadOnlyDictionary<string, string> DeniedAttributes { get; }
    public IReadOnlyCollection<string> AllowedOrganizationUnitIds { get; }
    public bool RequiresApproval { get; }
    public string? ApprovedByUserId { get; }

    public static AccessDecisionRequest ForAuthorizationRequirement(
        string moduleCode,
        IReadOnlyCollection<string> requiredRoles,
        IReadOnlyCollection<string> requiredPermissions)
    {
        var action = requiredRoles.Count == 0 && requiredPermissions.Count == 0
            ? AccessAction.CanView
            : AccessAction.CanExecute;

        return new AccessDecisionRequest(
            action,
            moduleCode,
            "AuthorizationRequirement",
            moduleCode,
            requiredRoles,
            requiredPermissions);
    }

    private static string Normalize(string value) =>
        string.IsNullOrWhiteSpace(value) ? "System" : value.Trim();

    private static IReadOnlyCollection<string> NormalizeCollection(IReadOnlyCollection<string>? values) =>
        values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    private static IReadOnlyDictionary<string, string> NormalizeDictionary(IReadOnlyDictionary<string, string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return values
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToDictionary(
                item => item.Key.Trim(),
                item => item.Value?.Trim() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);
    }
}
