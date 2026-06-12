namespace Lumindex.Infrastructure.Ingestion;

/// <summary>
/// Binds the <c>Azure:OpenAI</c> configuration section. When <see cref="Endpoint"/> and
/// <see cref="ApiKey"/> are set, embeddings are generated via Azure OpenAI; otherwise the pipeline
/// falls back to a deterministic local generator so it runs without any cloud dependency.
/// </summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "Azure:OpenAI";

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public string ChatDeployment { get; set; } = "gpt-4o-mini";

    public string EmbeddingDeployment { get; set; } = "text-embedding-3-small";

    /// <summary>Embedding vector size. Must match the search index's vector field dimensions.</summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(ApiKey);
}
