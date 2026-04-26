using System.Collections.Concurrent;

namespace SWFC.Infrastructure.M400_Integration.M407_DMSFileIntegration;

public sealed record DmsExternalDocumentRequest(
    string DmsSystem,
    string ExternalDocumentId,
    Uri ExternalUri,
    string FileName,
    string ContentType,
    long SizeBytes,
    string OwnerModule,
    string OwnerObjectType,
    string OwnerObjectId);

public sealed record ExternalDocumentReference(
    string DmsSystem,
    string ExternalDocumentId,
    Uri ExternalUri,
    string FileName,
    string ContentType,
    long SizeBytes,
    string OwnerModule,
    string OwnerObjectType,
    string OwnerObjectId,
    string SwfcReferenceKey);

public interface IDocumentReferenceSink
{
    Task RegisterReferenceAsync(
        ExternalDocumentReference reference,
        CancellationToken cancellationToken = default);
}

public sealed class InProcessDocumentReferenceSink : IDocumentReferenceSink
{
    private readonly ConcurrentQueue<ExternalDocumentReference> _references = new();

    public IReadOnlyCollection<ExternalDocumentReference> References => _references.ToArray();

    public Task RegisterReferenceAsync(
        ExternalDocumentReference reference,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);

        _references.Enqueue(reference);
        return Task.CompletedTask;
    }
}

public sealed class DmsFileReferenceAdapter
{
    private readonly IDocumentReferenceSink _documentReferenceSink;

    public DmsFileReferenceAdapter(IDocumentReferenceSink documentReferenceSink)
    {
        _documentReferenceSink = documentReferenceSink;
    }

    public async Task<ExternalDocumentReference> LinkAsync(
        DmsExternalDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        var reference = new ExternalDocumentReference(
            request.DmsSystem.Trim(),
            request.ExternalDocumentId.Trim(),
            request.ExternalUri,
            request.FileName.Trim(),
            request.ContentType.Trim(),
            request.SizeBytes,
            request.OwnerModule.Trim(),
            request.OwnerObjectType.Trim(),
            request.OwnerObjectId.Trim(),
            $"{request.OwnerModule.Trim()}:{request.OwnerObjectType.Trim()}:{request.OwnerObjectId.Trim()}:{request.ExternalDocumentId.Trim()}");

        await _documentReferenceSink.RegisterReferenceAsync(reference, cancellationToken);
        return reference;
    }

    private static void Validate(DmsExternalDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DmsSystem))
            throw new InvalidOperationException("DMS system is required.");

        if (string.IsNullOrWhiteSpace(request.ExternalDocumentId))
            throw new InvalidOperationException("External document id is required.");

        if (!request.ExternalUri.IsAbsoluteUri)
            throw new InvalidOperationException("External DMS URI must be absolute.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new InvalidOperationException("File name is required.");

        if (string.IsNullOrWhiteSpace(request.ContentType))
            throw new InvalidOperationException("Content type is required.");

        if (request.SizeBytes <= 0)
            throw new InvalidOperationException("Document size must be positive.");

        if (string.IsNullOrWhiteSpace(request.OwnerModule) ||
            string.IsNullOrWhiteSpace(request.OwnerObjectType) ||
            string.IsNullOrWhiteSpace(request.OwnerObjectId))
        {
            throw new InvalidOperationException("SWFC owner reference is required.");
        }
    }
}
