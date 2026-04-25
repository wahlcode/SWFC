namespace SWFC.Web.Pages.M300_Presentation.M305_Forms.Models;

public enum FormMode
{
    Create = 1,
    Edit = 2,
    ReadOnly = 3,
    Confirm = 4
}

public static class FormModeExtensions
{
    public static bool IsEditable(this FormMode mode)
    {
        return mode is FormMode.Create or FormMode.Edit or FormMode.Confirm;
    }
}
