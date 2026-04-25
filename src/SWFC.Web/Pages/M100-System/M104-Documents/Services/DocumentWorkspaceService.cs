using SWFC.Web.Pages.M100_System.M104_Documents.Models;

namespace SWFC.Web.Pages.M100_System.M104_Documents.Services;

public sealed class DocumentWorkspaceService
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, MutableDocument> _documents = new();

    public SwfcDocumentRecord RegisterDocument(DocumentRegistrationRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OwnerModule);

        var timestamp = DateTime.UtcNow;
        var document = new MutableDocument
        {
            Id = Guid.NewGuid(),
            Title = Normalize(request.Title),
            Category = NormalizeOptional(request.Category, "General"),
            OwnerModule = Normalize(request.OwnerModule).ToUpperInvariant(),
            RetentionPolicy = NormalizeOptional(request.RetentionPolicy, "Versioned retention"),
            CreatedUtc = timestamp,
            UpdatedUtc = timestamp
        };

        document.History.Add(new DocumentHistoryEntry(
            "Registered",
            timestamp,
            $"Document '{document.Title}' registered for {document.OwnerModule}."));

        lock (_gate)
        {
            _documents.Add(document.Id, document);
            return document.ToRecord();
        }
    }

    public SwfcDocumentRecord AddVersion(DocumentVersionRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Reason);

        if (request.SizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Document version size must be positive.");
        }

        var timestamp = DateTime.UtcNow;

        lock (_gate)
        {
            var document = GetMutableDocument(request.DocumentId);
            var version = new DocumentVersionRecord(
                document.Versions.Count + 1,
                Normalize(request.FileName),
                NormalizeOptional(request.ContentType, "application/octet-stream"),
                request.SizeBytes,
                NormalizeOptional(request.Checksum, "checksum-not-provided"),
                timestamp,
                Normalize(request.Reason));

            document.Versions.Add(version);
            document.UpdatedUtc = timestamp;
            document.History.Add(new DocumentHistoryEntry(
                "VersionAdded",
                timestamp,
                $"Version {version.VersionNumber} added: {version.FileName}."));

            return document.ToRecord();
        }
    }

    public SwfcDocumentRecord LinkDocument(DocumentLinkRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TargetModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TargetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TargetId);

        var timestamp = DateTime.UtcNow;
        var targetModule = Normalize(request.TargetModule).ToUpperInvariant();
        var targetType = Normalize(request.TargetType);
        var targetId = Normalize(request.TargetId);

        lock (_gate)
        {
            var document = GetMutableDocument(request.DocumentId);
            var duplicate = document.Links.Any(link =>
                string.Equals(link.TargetModule, targetModule, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(link.TargetType, targetType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(link.TargetId, targetId, StringComparison.OrdinalIgnoreCase));

            if (!duplicate)
            {
                document.Links.Add(new DocumentLinkRecord(
                    targetModule,
                    targetType,
                    targetId,
                    NormalizeOptional(request.Relationship, "Reference"),
                    timestamp));
                document.UpdatedUtc = timestamp;
                document.History.Add(new DocumentHistoryEntry(
                    "Linked",
                    timestamp,
                    $"Linked to {targetModule}:{targetType}:{targetId}."));
            }

            return document.ToRecord();
        }
    }

    public IReadOnlyList<SwfcDocumentRecord> GetDocuments()
    {
        lock (_gate)
        {
            return _documents.Values
                .OrderByDescending(document => document.UpdatedUtc)
                .Select(document => document.ToRecord())
                .ToArray();
        }
    }

    public SwfcDocumentRecord? GetDocument(Guid id)
    {
        lock (_gate)
        {
            return _documents.TryGetValue(id, out var document)
                ? document.ToRecord()
                : null;
        }
    }

    private MutableDocument GetMutableDocument(Guid id)
    {
        if (!_documents.TryGetValue(id, out var document))
        {
            throw new InvalidOperationException($"Document '{id}' was not found.");
        }

        return document;
    }

    private static string Normalize(string value) => value.Trim();

    private static string NormalizeOptional(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private sealed class MutableDocument
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string OwnerModule { get; init; } = string.Empty;
        public string RetentionPolicy { get; init; } = string.Empty;
        public DateTime CreatedUtc { get; init; }
        public DateTime UpdatedUtc { get; set; }
        public List<DocumentVersionRecord> Versions { get; } = new();
        public List<DocumentLinkRecord> Links { get; } = new();
        public List<DocumentHistoryEntry> History { get; } = new();

        public SwfcDocumentRecord ToRecord()
        {
            return new SwfcDocumentRecord(
                Id,
                Title,
                Category,
                OwnerModule,
                RetentionPolicy,
                CreatedUtc,
                UpdatedUtc,
                Versions.ToArray(),
                Links.ToArray(),
                History.ToArray());
        }
    }
}
