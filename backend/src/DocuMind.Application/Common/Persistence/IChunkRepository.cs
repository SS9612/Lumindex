using DocuMind.Domain.Entities;

namespace DocuMind.Application.Common.Persistence;

/// <summary>
/// Data access for <see cref="Chunk"/> entities produced by the ingestion pipeline. Reads are
/// scoped by <c>ownerId</c> to preserve per-user isolation even within background jobs.
/// </summary>
public interface IChunkRepository
{
    /// <summary>Returns a document's chunks ordered by their position within the source document.</summary>
    Task<IReadOnlyList<Chunk>> ListByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<Chunk> chunks);

    /// <summary>Removes every chunk belonging to a document (used when re-indexing or deleting).</summary>
    Task RemoveByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default);
}
