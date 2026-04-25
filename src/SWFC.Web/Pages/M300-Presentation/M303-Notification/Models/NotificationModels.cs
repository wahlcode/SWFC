namespace SWFC.Web.Pages.M300_Presentation.M303_Notification.Models;

public enum NotificationState
{
    Unread,
    Read,
    Done
}

public sealed record NotificationRecord(
    Guid Id,
    string Title,
    string Message,
    string Severity,
    string Priority,
    string TargetKind,
    string TargetValue,
    string Channel,
    string RelatedModule,
    string RelatedObjectId,
    NotificationState State,
    DateTime CreatedUtc,
    DateTime? EscalatedUtc);

public sealed record NotificationRequest(
    string Title,
    string Message,
    string Severity,
    string Priority,
    string TargetKind,
    string TargetValue,
    string Channel,
    string RelatedModule,
    string RelatedObjectId);
