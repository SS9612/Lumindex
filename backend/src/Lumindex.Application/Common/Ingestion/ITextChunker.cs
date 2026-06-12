namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Splits extracted pages into token-bounded, overlapping chunks suitable for embedding and retrieval.
/// </summary>
public interface ITextChunker
{
    IReadOnlyList<TextChunk> Chunk(IReadOnlyList<DocumentPage> pages);
}
