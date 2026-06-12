namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Runs the full ingestion pipeline for a single document: download → extract → chunk → embed → index,
/// updating the document's status as it progresses. Invoked by the background job runner.
/// </summary>
public interface IDocumentIngestionService
{
    Task IngestAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default);
}
