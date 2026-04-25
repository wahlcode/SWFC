using System.Text;
using System.Text.Json;

namespace SWFC.Web.Components.Layout;

public sealed class SidebarConfigService
{
    private readonly IWebHostEnvironment _environment;

    public SidebarConfigService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IReadOnlyList<SidebarGroupItem>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_environment.WebRootPath, "data", "sidebar.json");

        if (!File.Exists(filePath))
        {
            return Array.Empty<SidebarGroupItem>();
        }

        var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

        var file = JsonSerializer.Deserialize<SidebarFileDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var groups = file?.Groups?.ToList() ?? new List<SidebarGroupItem>();

        foreach (var group in groups)
        {
            group.DisplayNameKey = string.IsNullOrWhiteSpace(group.DisplayNameKey)
                ? $"Navigation.Group.{group.Key}"
                : group.DisplayNameKey.Trim();

            foreach (var module in group.Modules)
            {
                module.DisplayNameKey = string.IsNullOrWhiteSpace(module.DisplayNameKey)
                    ? $"Navigation.Module.{module.Code}"
                    : module.DisplayNameKey.Trim();

                module.AccessMode = NormalizeAccessMode(module.AccessMode);

                if (module.AccessMode == SidebarAccessModes.PermissionModule)
                {
                    module.AccessModuleCodes = module.AccessModuleCodes
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (module.AccessModuleCodes.Count == 0 && !string.IsNullOrWhiteSpace(module.Code))
                    {
                        module.AccessModuleCodes.Add(module.Code.Trim());
                    }
                }
                else
                {
                    module.AccessModuleCodes.Clear();
                }
            }
        }

        return groups;
    }

    private static string NormalizeAccessMode(string? accessMode)
    {
        if (string.Equals(accessMode, SidebarAccessModes.Authenticated, StringComparison.OrdinalIgnoreCase))
        {
            return SidebarAccessModes.Authenticated;
        }

        return SidebarAccessModes.PermissionModule;
    }
}