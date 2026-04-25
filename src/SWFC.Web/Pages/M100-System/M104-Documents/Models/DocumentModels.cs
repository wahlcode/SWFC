namespace SWFC.Web.Pages.M100_System.M104_Documents.Models;

public sealed record DocumentRegistrationRequest(
    string Title,
    string Category,
    string OwnerModule,
    string RetentionPolicy);

public sealed record DocumentVersionRequest(
    Guid DocumentId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Checksum,
    string Reason);

public sealed record DocumentLinkRequest(
    Guid DocumentId,
    string TargetModule,
    string TargetType,
    string TargetId,
    string Relationship);

public sealed record DocumentVersionRecord(
    int VersionNumber,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Checksum,
    DateTime CreatedUtc,
    string Reason);

public sealed record DocumentLinkRecord(
    string TargetModule,
    string TargetType,
    string TargetId,
    string Relationship,
    DateTime LinkedUtc);

public sealed record DocumentHistoryEntry(
    string Action,
    DateTime TimestampUtc,
    string Summary);

public sealed record SwfcDocumentRecord(
    Guid Id,
    string Title,
    string Category,
    string OwnerModule,
    string RetentionPolicy,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    IReadOnlyList<DocumentVersionRecord> Versions,
    IReadOnlyList<DocumentLinkRecord> Links,
    IReadOnlyList<DocumentHistoryEntry> History);
