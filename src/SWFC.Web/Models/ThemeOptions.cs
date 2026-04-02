namespace SWFC.Web.Models;

public sealed class ThemeOptions
{
    public string BackgroundColor { get; set; } = "#121212";
    public string SurfaceColor { get; set; } = "#1e1e1e";
    public string SurfaceAltColor { get; set; } = "#2a2a2a";
    public string BorderColor { get; set; } = "#3f3f3f";

    public string StatusOpen { get; set; } = "#444444";
    public string StatusInProgress { get; set; } = "#b58900";
    public string StatusDone { get; set; } = "#1f7a3a";
}