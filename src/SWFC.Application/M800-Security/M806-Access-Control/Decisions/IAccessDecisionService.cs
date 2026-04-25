using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Application.M800_Security.M806_AccessControl.Decisions;

public interface IAccessDecisionService
{
    Task<AccessDecisionResult> DecideAsync(
        SecurityContext securityContext,
        AccessDecisionRequest request,
        CancellationToken cancellationToken = default);
}
