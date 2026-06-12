namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// A chunk plus its embedding and per-user metadata, ready to be upserted into the vector index.
/// <see cref="OwnerId"/> is stored as a filterable field so retrieval can enforce per-user isolation.
/// </summary>
public sealed record IndexedChunk(
    Guid ChunkId,
    Guid OwnerId,
    Guid DocumentId,
    int Ordinal,
    int? PageNumber,
    string Content,
    ReadOnlyMemory<float> Embedding);
