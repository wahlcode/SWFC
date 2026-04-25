using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines;

public sealed class AccessRuleEditorModel
{
    public AccessSubjectType SubjectType { get; set; } = AccessSubjectType.User;
    public string SubjectId { get; set; } = string.Empty;
    public AccessRuleMode Mode { get; set; } = AccessRuleMode.Allow;
    public string Reason { get; set; } = string.Empty;

    public void Reset()
    {
        SubjectType = AccessSubjectType.User;
        SubjectId = string.Empty;
        Mode = AccessRuleMode.Allow;
        Reason = string.Empty;
    }
}
