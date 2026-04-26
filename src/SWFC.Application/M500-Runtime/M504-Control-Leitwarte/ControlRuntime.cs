namespace SWFC.Application.M500_Runtime.M504_Control_Leitwarte;

public enum ControlDeskDecisionStatus
{
    Accepted = 0,
    Rejected = 1
}

public sealed record ControlDeskSnapshot(
    string AreaId,
    DateTime ObservedAtUtc,
    IReadOnlyDictionary<string, string> LiveStates,
    IReadOnlyList<string> ActiveAlarms);

public sealed record ControlCommandRequest(
    string CommandId,
    string AreaId,
    string TargetId,
    string Operation,
    string RequestedByUserId,
    DateTime RequestedAtUtc,
    bool HasPermission,
    bool HasSafetyClearance,
    string? ApprovalReference,
    string? LocalConfirmationReference);

public sealed record ControlCommandDecision(
    string CommandId,
    ControlDeskDecisionStatus Status,
    string Reason,
    DateTime DecidedAtUtc);

public interface IControlDeskRuntime
{
    ControlDeskSnapshot PublishSnapshot(ControlDeskSnapshot snapshot);

    ControlCommandDecision EvaluateCommand(ControlCommandRequest request);

    IReadOnlyList<ControlDeskSnapshot> GetSnapshots();

    IReadOnlyList<ControlCommandDecision> GetDecisions();
}

public sealed class ControlDeskRuntime : IControlDeskRuntime
{
    private readonly Dictionary<string, ControlDeskSnapshot> _snapshots = new(StringComparer.Ordinal);
    private readonly List<ControlCommandDecision> _decisions = [];

    public ControlDeskSnapshot PublishSnapshot(ControlDeskSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.IsNullOrWhiteSpace(snapshot.AreaId))
        {
            throw new ArgumentException("Area id is required.");
        }

        var normalized = snapshot with
        {
            AreaId = snapshot.AreaId.Trim(),
            LiveStates = ToSortedDictionary(snapshot.LiveStates),
            ActiveAlarms = snapshot.ActiveAlarms?.Order(StringComparer.Ordinal).ToArray() ?? []
        };

        _snapshots[normalized.AreaId] = normalized;
        return normalized;
    }

    public ControlCommandDecision EvaluateCommand(ControlCommandRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var decision = ValidateCommand(request);
        _decisions.Add(decision);
        return decision;
    }

    public IReadOnlyList<ControlDeskSnapshot> GetSnapshots() =>
        _snapshots.Values.OrderBy(x => x.AreaId, StringComparer.Ordinal).ToArray();

    public IReadOnlyList<ControlCommandDecision> GetDecisions() => _decisions.ToArray();

    private static ControlCommandDecision ValidateCommand(ControlCommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) ||
            string.IsNullOrWhiteSpace(request.AreaId) ||
            string.IsNullOrWhiteSpace(request.TargetId) ||
            string.IsNullOrWhiteSpace(request.Operation) ||
            string.IsNullOrWhiteSpace(request.RequestedByUserId))
        {
            return Reject(request, "Command request is incomplete.");
        }

        if (!request.HasPermission)
        {
            return Reject(request, "Permission is missing.");
        }

        if (!request.HasSafetyClearance)
        {
            return Reject(request, "Safety clearance is missing.");
        }

        if (string.IsNullOrWhiteSpace(request.ApprovalReference))
        {
            return Reject(request, "Approval reference is missing.");
        }

        if (string.IsNullOrWhiteSpace(request.LocalConfirmationReference))
        {
            return Reject(request, "Local confirmation reference is missing.");
        }

        return new ControlCommandDecision(
            request.CommandId.Trim(),
            ControlDeskDecisionStatus.Accepted,
            "Control command accepted for controlled execution.",
            request.RequestedAtUtc);
    }

    private static ControlCommandDecision Reject(ControlCommandRequest request, string reason) =>
        new(
            string.IsNullOrWhiteSpace(request.CommandId) ? "unknown" : request.CommandId.Trim(),
            ControlDeskDecisionStatus.Rejected,
            reason,
            request.RequestedAtUtc);

    private static SortedDictionary<string, string> ToSortedDictionary(
        IReadOnlyDictionary<string, string>? values) =>
        values is null
            ? new SortedDictionary<string, string>(StringComparer.Ordinal)
            : new SortedDictionary<string, string>(
                values.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal),
                StringComparer.Ordinal);
}
