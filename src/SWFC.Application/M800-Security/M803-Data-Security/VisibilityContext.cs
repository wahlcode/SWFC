namespace SWFC.Application.M800_Security.M803_Visibility;

public sealed record VisibilityContext(
    string UserId,
    IReadOnlyCollection<string> RoleIds,
    IReadOnlyCollection<string> OrganizationUnitIds)
{
    public static VisibilityContext Create(
        string userId,
        IReadOnlyCollection<string>? roleIds,
        IReadOnlyCollection<string>? organizationUnitIds)
    {
        return new VisibilityContext(
            NormalizeRequired(userId),
            NormalizeMany(roleIds),
            NormalizeMany(organizationUnitIds));
    }

    private static string NormalizeRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("UserId is required.", nameof(value));
        }

        return value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeMany(IReadOnlyCollection<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return Array.Empty<string>();
        }

        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
