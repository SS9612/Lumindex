namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Enqueues a document for asynchronous ingestion. Abstracts the background job scheduler (Hangfire)
/// so the application layer never depends on the concrete queue implementation.
/// </summary>
public interface IDocumentIngestionQueue
{
    void Enqueue(Guid ownerId, Guid documentId);
}
