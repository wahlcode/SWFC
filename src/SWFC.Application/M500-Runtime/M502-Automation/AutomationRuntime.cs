namespace SWFC.Application.M500_Runtime.M502_Automation;

public enum AutomationActionKind
{
    EnqueueJob = 0,
    Notify = 1,
    RequestControlApproval = 2,
    ControlCommand = 3
}

public sealed record AutomationEvent(
    string EventName,
    string SourceModule,
    string SourceObjectId,
    DateTime OccurredAtUtc,
    IReadOnlyDictionary<string, string> Facts);

public sealed record AutomationCondition(string FactKey, string ExpectedValue);

public sealed record AutomationAction(
    AutomationActionKind Kind,
    string TargetModule,
    string TargetOperation,
    IReadOnlyDictionary<string, string> Parameters,
    bool IsCritical = false);

public sealed record AutomationRule(
    string RuleId,
    string Name,
    string TriggerEventName,
    IReadOnlyList<AutomationCondition> Conditions,
    IReadOnlyList<AutomationAction> Actions,
    bool IsEnabled = true,
    string? ApprovalReference = null);

public sealed record AutomationDecision(
    string RuleId,
    string EventName,
    bool Matched,
    bool Blocked,
    string StatusReason,
    IReadOnlyList<AutomationAction> PlannedActions,
    DateTime DecidedAtUtc);

public interface IAutomationRuleEngine
{
    AutomationRule Register(AutomationRule rule);

    AutomationRule Disable(string ruleId);

    AutomationRule Enable(string ruleId);

    IReadOnlyList<AutomationDecision> Evaluate(AutomationEvent automationEvent);

    IReadOnlyList<AutomationRule> GetRules();

    IReadOnlyList<AutomationDecision> GetDecisionHistory();
}

public sealed class AutomationRuleEngine : IAutomationRuleEngine
{
    private readonly Dictionary<string, AutomationRule> _rules = new(StringComparer.Ordinal);
    private readonly List<AutomationDecision> _history = [];

    public AutomationRule Register(AutomationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        Validate(rule);
        var normalized = Normalize(rule);
        _rules[normalized.RuleId] = normalized;
        return normalized;
    }

    public AutomationRule Disable(string ruleId) => ChangeState(ruleId, isEnabled: false);

    public AutomationRule Enable(string ruleId) => ChangeState(ruleId, isEnabled: true);

    public IReadOnlyList<AutomationDecision> Evaluate(AutomationEvent automationEvent)
    {
        ArgumentNullException.ThrowIfNull(automationEvent);
        Validate(automationEvent);

        var decisions = new List<AutomationDecision>();

        foreach (var rule in _rules.Values.OrderBy(x => x.RuleId, StringComparer.Ordinal))
        {
            var decision = EvaluateRule(rule, automationEvent);
            decisions.Add(decision);
            _history.Add(decision);
        }

        return decisions;
    }

    public IReadOnlyList<AutomationRule> GetRules() =>
        _rules.Values.OrderBy(x => x.RuleId, StringComparer.Ordinal).ToArray();

    public IReadOnlyList<AutomationDecision> GetDecisionHistory() => _history.ToArray();

    private AutomationRule ChangeState(string ruleId, bool isEnabled)
    {
        if (!_rules.TryGetValue(ruleId, out var rule))
        {
            throw new InvalidOperationException($"Automation rule '{ruleId}' is not registered.");
        }

        var changed = rule with { IsEnabled = isEnabled };
        _rules[ruleId] = changed;
        return changed;
    }

    private static void Validate(AutomationRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.RuleId) ||
            string.IsNullOrWhiteSpace(rule.Name) ||
            string.IsNullOrWhiteSpace(rule.TriggerEventName))
        {
            throw new ArgumentException("Rule id, name and trigger event are required.");
        }

        if (rule.Actions is null || rule.Actions.Count == 0)
        {
            throw new ArgumentException("Automation rules require at least one action.");
        }
    }

    private static void Validate(AutomationEvent automationEvent)
    {
        if (string.IsNullOrWhiteSpace(automationEvent.EventName) ||
            string.IsNullOrWhiteSpace(automationEvent.SourceModule) ||
            string.IsNullOrWhiteSpace(automationEvent.SourceObjectId))
        {
            throw new ArgumentException("Event name, source module and source object id are required.");
        }
    }

    private static AutomationRule Normalize(AutomationRule rule) =>
        rule with
        {
            RuleId = rule.RuleId.Trim(),
            Name = rule.Name.Trim(),
            TriggerEventName = rule.TriggerEventName.Trim(),
            Conditions = rule.Conditions?.ToArray() ?? [],
            Actions = rule.Actions.Select(action => action with
            {
                TargetModule = action.TargetModule.Trim(),
                TargetOperation = action.TargetOperation.Trim(),
                Parameters = ToSortedDictionary(action.Parameters)
            }).ToArray(),
            ApprovalReference = string.IsNullOrWhiteSpace(rule.ApprovalReference)
                ? null
                : rule.ApprovalReference.Trim()
        };

    private static SortedDictionary<string, string> ToSortedDictionary(
        IReadOnlyDictionary<string, string>? values) =>
        values is null
            ? new SortedDictionary<string, string>(StringComparer.Ordinal)
            : new SortedDictionary<string, string>(
                values.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal),
                StringComparer.Ordinal);

    private static AutomationDecision EvaluateRule(AutomationRule rule, AutomationEvent automationEvent)
    {
        if (!rule.IsEnabled)
        {
            return BuildDecision(rule, automationEvent, matched: false, blocked: false, [], "Rule disabled.");
        }

        if (!string.Equals(rule.TriggerEventName, automationEvent.EventName, StringComparison.Ordinal))
        {
            return BuildDecision(rule, automationEvent, matched: false, blocked: false, [], "Trigger event does not match.");
        }

        var conditionsMatch = rule.Conditions.All(condition =>
            (automationEvent.Facts ?? new Dictionary<string, string>()).TryGetValue(condition.FactKey, out var actual) &&
            string.Equals(actual, condition.ExpectedValue, StringComparison.Ordinal));

        if (!conditionsMatch)
        {
            return BuildDecision(rule, automationEvent, matched: false, blocked: false, [], "Conditions do not match.");
        }

        var hasUnsafeCriticalAction = rule.Actions.Any(action =>
            action.IsCritical &&
            action.Kind == AutomationActionKind.ControlCommand &&
            string.IsNullOrWhiteSpace(rule.ApprovalReference));

        if (hasUnsafeCriticalAction)
        {
            return BuildDecision(
                rule,
                automationEvent,
                matched: true,
                blocked: true,
                [],
                "Critical control action requires an approval reference.");
        }

        return BuildDecision(rule, automationEvent, matched: true, blocked: false, rule.Actions, "Actions planned.");
    }

    private static AutomationDecision BuildDecision(
        AutomationRule rule,
        AutomationEvent automationEvent,
        bool matched,
        bool blocked,
        IReadOnlyList<AutomationAction> plannedActions,
        string reason) =>
        new(
            rule.RuleId,
            automationEvent.EventName,
            matched,
            blocked,
            reason,
            plannedActions.ToArray(),
            automationEvent.OccurredAtUtc);
}
