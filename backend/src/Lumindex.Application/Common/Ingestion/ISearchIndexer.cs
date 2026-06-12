namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Writes chunk embeddings + metadata into the vector search index. Reads (hybrid retrieval) are added
/// in the chat feature; ingestion only needs upsert and per-document delete for re-indexing/cleanup.
/// </summary>
public interface ISearchIndexer
{
    /// <summary>Creates/updates the given chunks in the index (idempotent by chunk id).</summary>
    Task UpsertAsync(IReadOnlyList<IndexedChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>Removes every indexed chunk for a document, scoped to its owner.</summary>
    Task DeleteByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default);
}
