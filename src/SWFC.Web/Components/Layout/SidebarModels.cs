namespace SWFC.Web.Components.Layout;

public sealed class SidebarFileDto
{
    public List<SidebarGroupItem> Groups { get; set; } = new();
}

public sealed class SidebarGroupItem
{
    public string Key { get; set; } = string.Empty;
    public string DisplayNameKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<SidebarModuleItem> Modules { get; set; } = new();
}

public sealed class SidebarModuleItem
{
    public string Code { get; set; } = string.Empty;
    public string DisplayNameKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Route { get; set; } = "#";
    public string AccessMode { get; set; } = SidebarAccessModes.PermissionModule;
    public List<string> AccessModuleCodes { get; set; } = new();
}

public static class SidebarAccessModes
{
    public const string Authenticated = "authenticated";
    public const string PermissionModule = "permission-module";
}