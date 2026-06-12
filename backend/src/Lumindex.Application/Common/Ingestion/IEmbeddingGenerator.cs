namespace Lumindex.Application.Common.Ingestion;

/// <summary>
/// Turns text into dense vector embeddings for semantic search. Implemented over Azure OpenAI in
/// production and a deterministic local generator for offline development/tests.
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>Dimensionality of the vectors produced; must match the search index's vector field.</summary>
    int Dimensions { get; }

    /// <summary>
    /// Generates one embedding per input, preserving order. Returns an empty list for empty input.
    /// </summary>
    Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default);
}
