using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines.Components.ViewModels;

public sealed class AccessRuleVm
{
    public Guid Id { get; init; }

    public AccessSubjectType SubjectType { get; init; }
    public string SubjectId { get; init; } = string.Empty;

    public AccessRuleMode Mode { get; init; }
}
