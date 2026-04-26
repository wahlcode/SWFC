using SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

namespace SWFC.Web.Pages.M100_System.M102_Organization.OrganizationUnits;

public sealed class OrganizationUnitsText
{
    public string Back { get; private set; } = "Back";
    public string Eyebrow { get; private set; } = "M102 · Organization";
    public string Title { get; private set; } = "Organization Units";
    public string Subtitle { get; private set; } = "Organizational parent/child structure using real M102 name, code and parent data.";
    public string Total { get; private set; } = "Total";
    public string Active { get; private set; } = "Active";
    public string Inactive { get; private set; } = "Inactive";
    public string Roots { get; private set; } = "Roots";
    public string Matches { get; private set; } = "Matches";
    public string SearchLabel { get; private set; } = "Search name or code";
    public string NewUnit { get; private set; } = "New unit";
    public string Loading { get; private set; } = "Loading organization units...";
    public string Empty { get; private set; } = "No organization units found.";
    public string Details { get; private set; } = "Details";
    public string NewTitle { get; private set; } = "New organization unit";
    public string CreateInfo { get; private set; } = "The assignment to a parent unit uses the existing parent structure.";
    public string Name { get; private set; } = "Name";
    public string Code { get; private set; } = "Code";
    public string ParentUnit { get; private set; } = "Parent unit";
    public string NoParent { get; private set; } = "-- no parent --";
    public string Reason { get; private set; } = "Reason";
    public string Save { get; private set; } = "Save";
    public string Cancel { get; private set; } = "Cancel";
    public string Created { get; private set; } = "Organization unit created.";
    public string ChildUnitSingular { get; private set; } = "child unit";
    public string ChildUnitPlural { get; private set; } = "child units";

    public async Task LoadAsync(LocalizationTextProvider textProvider)
    {
        var culture = LocalizationTextProvider.DefaultCultureName;

        Back = await textProvider.GetTextAsync("Common.Back", culture);
        Eyebrow = await textProvider.GetTextAsync("OrganizationUnitDetail.Eyebrow", culture);
        Title = await textProvider.GetTextAsync("OrganizationUnits.Title", culture);
        Subtitle = await textProvider.GetTextAsync("OrganizationUnits.Subtitle", culture);
        Total = await textProvider.GetTextAsync("OrganizationUnits.Total", culture);
        Active = await textProvider.GetTextAsync("Common.Active", culture);
        Inactive = await textProvider.GetTextAsync("Common.Inactive", culture);
        Roots = await textProvider.GetTextAsync("OrganizationUnits.Roots", culture);
        Matches = await textProvider.GetTextAsync("OrganizationUnits.Matches", culture);
        SearchLabel = await textProvider.GetTextAsync("OrganizationUnits.SearchLabel", culture);
        NewUnit = await textProvider.GetTextAsync("OrganizationUnits.NewUnit", culture);
        Loading = await textProvider.GetTextAsync("OrganizationUnits.Loading", culture);
        Empty = await textProvider.GetTextAsync("OrganizationUnits.Empty", culture);
        Details = await textProvider.GetTextAsync("Common.Details", culture);
        NewTitle = await textProvider.GetTextAsync("OrganizationUnitDetail.NewTitle", culture);
        CreateInfo = await textProvider.GetTextAsync("OrganizationUnits.CreateInfo", culture);
        Name = await textProvider.GetTextAsync("Common.Name", culture);
        Code = await textProvider.GetTextAsync("Common.Code", culture);
        ParentUnit = await textProvider.GetTextAsync("OrganizationUnitDetail.ParentUnit", culture);
        NoParent = await textProvider.GetTextAsync("Common.NoParent", culture);
        Reason = await textProvider.GetTextAsync("Common.Reason", culture);
        Save = await textProvider.GetTextAsync("Common.Save", culture);
        Cancel = await textProvider.GetTextAsync("Common.Cancel", culture);
        Created = await textProvider.GetTextAsync("OrganizationUnitDetail.Created", culture);
        ChildUnitSingular = await textProvider.GetTextAsync("OrganizationUnits.ChildUnitSingular", culture);
        ChildUnitPlural = await textProvider.GetTextAsync("OrganizationUnits.ChildUnitPlural", culture);
    }
}