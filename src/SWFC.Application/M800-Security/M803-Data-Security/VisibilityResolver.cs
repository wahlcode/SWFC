using SWFC.Application.M800_Security.M801_Access;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Application.M800_Security.M803_Visibility;

public interface IVisibilityResolver
{
    Task<bool> CanViewMachine(
        VisibilityContext context,
        Guid machineId,
        CancellationToken cancellationToken = default);

    Task<bool> CanViewArea(
        VisibilityContext context,
        Guid areaId,
        CancellationToken cancellationToken = default);

    Task<bool> CanViewComponent(
        VisibilityContext context,
        Guid componentId,
        CancellationToken cancellationToken = default);
}

public sealed class VisibilityResolver : IVisibilityResolver
{
    private readonly IAccessRuleReadRepository _accessRuleReadRepository;
    private readonly IVisibilityTargetContextRepository _targetContextRepository;

    public VisibilityResolver(
        IAccessRuleReadRepository accessRuleReadRepository,
        IVisibilityTargetContextRepository targetContextRepository)
    {
        _accessRuleReadRepository = accessRuleReadRepository;
        _targetContextRepository = targetContextRepository;
    }

    public async Task<bool> CanViewMachine(
        VisibilityContext context,
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var targetContext = await _targetContextRepository.GetMachineContextAsync(machineId, cancellationToken);

        if (targetContext is null)
        {
            return false;
        }

        var targetChain = new[]
        {
            new AccessRuleTargetRef(AccessTargetType.Machine, targetContext.MachineId.ToString())
        };

        var rules = await _accessRuleReadRepository.GetByTargetsAsync(targetChain, cancellationToken);

        return Resolve(rules, context, targetChain);
    }

    public async Task<bool> CanViewArea(
        VisibilityContext context,
        Guid areaId,
        CancellationToken cancellationToken = default)
    {
        var targetContext = await _targetContextRepository.GetAreaContextAsync(areaId, cancellationToken);

        if (targetContext is null)
        {
            return false;
        }

        var targetChain = new[]
        {
            new AccessRuleTargetRef(AccessTargetType.MachineComponentArea, targetContext.AreaId.ToString())
        };

        var rules = await _accessRuleReadRepository.GetByTargetsAsync(targetChain, cancellationToken);

        return Resolve(rules, context, targetChain);
    }

    public async Task<bool> CanViewComponent(
        VisibilityContext context,
        Guid componentId,
        CancellationToken cancellationToken = default)
    {
        var targetContext = await _targetContextRepository.GetComponentContextAsync(componentId, cancellationToken);

        if (targetContext is null)
        {
            return false;
        }

        var targetChain = new List<AccessRuleTargetRef>
        {
            new(AccessTargetType.MachineComponent, targetContext.ComponentId.ToString())
        };

        foreach (var parentComponentId in targetContext.ParentComponentIdsNearestFirst)
        {
            targetChain.Add(new AccessRuleTargetRef(
                AccessTargetType.MachineComponent,
                parentComponentId.ToString()));
        }

        if (targetContext.AreaId.HasValue)
        {
            targetChain.Add(new AccessRuleTargetRef(
                AccessTargetType.MachineComponentArea,
                targetContext.AreaId.Value.ToString()));
        }

        targetChain.Add(new AccessRuleTargetRef(
            AccessTargetType.Machine,
            targetContext.MachineId.ToString()));

        var rules = await _accessRuleReadRepository.GetByTargetsAsync(targetChain, cancellationToken);

        return Resolve(rules, context, targetChain);
    }

    private static bool Resolve(
        IReadOnlyCollection<AccessRule> rules,
        VisibilityContext context,
        IReadOnlyList<AccessRuleTargetRef> targetPriorityChain)
    {
        foreach (var target in targetPriorityChain)
        {
            var matchedRules = rules
                .Where(rule => rule.TargetType == target.TargetType)
                .Where(rule => string.Equals(rule.TargetId, target.TargetId, StringComparison.OrdinalIgnoreCase))
                .Where(rule => MatchesContext(rule, context))
                .ToList();

            if (matchedRules.Count == 0)
            {
                continue;
            }

            if (matchedRules.Any(x => x.Mode == AccessRuleMode.Deny))
            {
                return false;
            }

            if (matchedRules.Any(x => x.Mode == AccessRuleMode.Allow))
            {
                return true;
            }
        }

        return true;
    }

    private static bool MatchesContext(
        AccessRule rule,
        VisibilityContext context)
    {
        return rule.SubjectType switch
        {
            AccessSubjectType.User =>
                string.Equals(rule.SubjectId, context.UserId, StringComparison.OrdinalIgnoreCase),

            AccessSubjectType.Role =>
                context.RoleIds.Contains(rule.SubjectId, StringComparer.OrdinalIgnoreCase),

            AccessSubjectType.OrganizationUnit =>
                context.OrganizationUnitIds.Contains(rule.SubjectId, StringComparer.OrdinalIgnoreCase),

            _ => false
        };
    }
}
