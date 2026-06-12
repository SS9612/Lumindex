namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// A retrieval-sized slice of a document produced by the chunker. <see cref="PageNumber"/> records the
/// page the chunk starts on so the chat UI can deep-link citations back to the source.
/// </summary>
public sealed record TextChunk(int Ordinal, int? PageNumber, string Content, int TokenCount);
