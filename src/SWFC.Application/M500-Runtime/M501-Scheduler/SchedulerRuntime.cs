namespace SWFC.Application.M500_Runtime.M501_Scheduler;

public enum SchedulerTriggerKind
{
    Once = 0,
    Interval = 1,
    DueDate = 2
}

public enum SchedulerState
{
    Active = 0,
    Paused = 1,
    Stopped = 2
}

public sealed record SchedulerJobSchedule(
    string ScheduleId,
    string JobKey,
    SchedulerTriggerKind TriggerKind,
    DateTime StartsAtUtc,
    TimeSpan? Interval,
    DateTime? DueAtUtc,
    string SourceModule,
    IReadOnlyDictionary<string, string> Parameters,
    SchedulerState State = SchedulerState.Active,
    DateTime? LastTriggeredAtUtc = null);

public sealed record ScheduledRuntimeTrigger(
    string ScheduleId,
    string JobKey,
    string SourceModule,
    DateTime TriggeredAtUtc,
    string TriggerReason,
    IReadOnlyDictionary<string, string> Parameters);

public interface IRuntimeScheduler
{
    SchedulerJobSchedule Register(SchedulerJobSchedule schedule);

    SchedulerJobSchedule Pause(string scheduleId);

    SchedulerJobSchedule Resume(string scheduleId);

    SchedulerJobSchedule Stop(string scheduleId);

    IReadOnlyList<ScheduledRuntimeTrigger> Evaluate(DateTime utcNow);

    IReadOnlyList<SchedulerJobSchedule> GetSchedules();

    IReadOnlyList<ScheduledRuntimeTrigger> GetTriggerHistory();
}

public sealed class RuntimeScheduler : IRuntimeScheduler
{
    private readonly Dictionary<string, SchedulerJobSchedule> _schedules = new(StringComparer.Ordinal);
    private readonly List<ScheduledRuntimeTrigger> _history = [];

    public SchedulerJobSchedule Register(SchedulerJobSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        Validate(schedule);
        var normalized = Normalize(schedule);
        _schedules[normalized.ScheduleId] = normalized;
        return normalized;
    }

    public SchedulerJobSchedule Pause(string scheduleId) => ChangeState(scheduleId, SchedulerState.Paused);

    public SchedulerJobSchedule Resume(string scheduleId) => ChangeState(scheduleId, SchedulerState.Active);

    public SchedulerJobSchedule Stop(string scheduleId) => ChangeState(scheduleId, SchedulerState.Stopped);

    public IReadOnlyList<ScheduledRuntimeTrigger> Evaluate(DateTime utcNow)
    {
        var dueTriggers = new List<ScheduledRuntimeTrigger>();

        foreach (var schedule in _schedules.Values.OrderBy(x => x.ScheduleId, StringComparer.Ordinal))
        {
            if (schedule.State != SchedulerState.Active || !IsDue(schedule, utcNow))
            {
                continue;
            }

            var trigger = new ScheduledRuntimeTrigger(
                schedule.ScheduleId,
                schedule.JobKey,
                schedule.SourceModule,
                utcNow,
                BuildReason(schedule),
                ToSortedDictionary(schedule.Parameters));

            dueTriggers.Add(trigger);
            _history.Add(trigger);
            _schedules[schedule.ScheduleId] = schedule with { LastTriggeredAtUtc = utcNow };
        }

        return dueTriggers;
    }

    public IReadOnlyList<SchedulerJobSchedule> GetSchedules() =>
        _schedules.Values.OrderBy(x => x.ScheduleId, StringComparer.Ordinal).ToArray();

    public IReadOnlyList<ScheduledRuntimeTrigger> GetTriggerHistory() => _history.ToArray();

    private SchedulerJobSchedule ChangeState(string scheduleId, SchedulerState state)
    {
        if (!_schedules.TryGetValue(scheduleId, out var schedule))
        {
            throw new InvalidOperationException($"Schedule '{scheduleId}' is not registered.");
        }

        var changed = schedule with { State = state };
        _schedules[scheduleId] = changed;
        return changed;
    }

    private static void Validate(SchedulerJobSchedule schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule.ScheduleId) ||
            string.IsNullOrWhiteSpace(schedule.JobKey) ||
            string.IsNullOrWhiteSpace(schedule.SourceModule))
        {
            throw new ArgumentException("Schedule id, job key and source module are required.");
        }

        if (schedule.TriggerKind == SchedulerTriggerKind.Interval &&
            (!schedule.Interval.HasValue || schedule.Interval.Value <= TimeSpan.Zero))
        {
            throw new ArgumentException("Interval schedules require a positive interval.");
        }

        if (schedule.TriggerKind == SchedulerTriggerKind.DueDate && !schedule.DueAtUtc.HasValue)
        {
            throw new ArgumentException("Due date schedules require a due timestamp.");
        }
    }

    private static SchedulerJobSchedule Normalize(SchedulerJobSchedule schedule) =>
        schedule with
        {
            ScheduleId = schedule.ScheduleId.Trim(),
            JobKey = schedule.JobKey.Trim(),
            SourceModule = schedule.SourceModule.Trim(),
            Parameters = ToSortedDictionary(schedule.Parameters)
        };

    private static SortedDictionary<string, string> ToSortedDictionary(
        IReadOnlyDictionary<string, string>? values) =>
        values is null
            ? new SortedDictionary<string, string>(StringComparer.Ordinal)
            : new SortedDictionary<string, string>(
                values.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal),
                StringComparer.Ordinal);

    private static bool IsDue(SchedulerJobSchedule schedule, DateTime utcNow)
    {
        if (utcNow < schedule.StartsAtUtc)
        {
            return false;
        }

        return schedule.TriggerKind switch
        {
            SchedulerTriggerKind.Once => schedule.LastTriggeredAtUtc is null,
            SchedulerTriggerKind.Interval => schedule.LastTriggeredAtUtc is null ||
                                             utcNow - schedule.LastTriggeredAtUtc.Value >= schedule.Interval!.Value,
            SchedulerTriggerKind.DueDate => schedule.DueAtUtc <= utcNow &&
                                            schedule.LastTriggeredAtUtc is null,
            _ => false
        };
    }

    private static string BuildReason(SchedulerJobSchedule schedule) =>
        schedule.TriggerKind switch
        {
            SchedulerTriggerKind.Once => "Once schedule reached.",
            SchedulerTriggerKind.Interval => "Interval elapsed.",
            SchedulerTriggerKind.DueDate => "Due date reached.",
            _ => "Schedule reached."
        };
}
