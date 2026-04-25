using SWFC.Web.Pages.M300_Presentation.M303_Notification.Models;

namespace SWFC.Web.Pages.M300_Presentation.M303_Notification.Services;

public sealed class NotificationWorkspaceService
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, NotificationRecord> _notifications = new();

    public NotificationRecord Publish(NotificationRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Message);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TargetKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TargetValue);

        var notification = new NotificationRecord(
            Guid.NewGuid(),
            request.Title.Trim(),
            request.Message.Trim(),
            NormalizeChoice(request.Severity, "Info"),
            NormalizeChoice(request.Priority, "Medium"),
            request.TargetKind.Trim(),
            request.TargetValue.Trim(),
            NormalizeChoice(request.Channel, "UI"),
            NormalizeChoice(request.RelatedModule, "M303").ToUpperInvariant(),
            string.IsNullOrWhiteSpace(request.RelatedObjectId) ? "system" : request.RelatedObjectId.Trim(),
            NotificationState.Unread,
            DateTime.UtcNow,
            null);

        lock (_gate)
        {
            _notifications[notification.Id] = notification;
        }

        return notification;
    }

    public NotificationRecord MarkRead(Guid id) => UpdateState(id, NotificationState.Read);

    public NotificationRecord Complete(Guid id) => UpdateState(id, NotificationState.Done);

    public NotificationRecord Escalate(Guid id)
    {
        lock (_gate)
        {
            var current = GetExisting(id);
            var escalated = current with
            {
                Priority = "Critical",
                EscalatedUtc = DateTime.UtcNow
            };

            _notifications[id] = escalated;
            return escalated;
        }
    }

    public IReadOnlyList<NotificationRecord> GetNotifications()
    {
        lock (_gate)
        {
            return _notifications.Values
                .OrderBy(notification => notification.State)
                .ThenByDescending(notification => notification.CreatedUtc)
                .ToArray();
        }
    }

    private NotificationRecord UpdateState(Guid id, NotificationState state)
    {
        lock (_gate)
        {
            var current = GetExisting(id);
            var updated = current with { State = state };
            _notifications[id] = updated;
            return updated;
        }
    }

    private NotificationRecord GetExisting(Guid id)
    {
        if (!_notifications.TryGetValue(id, out var notification))
        {
            throw new InvalidOperationException($"Notification '{id}' was not found.");
        }

        return notification;
    }

    private static string NormalizeChoice(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
