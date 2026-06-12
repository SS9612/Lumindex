namespace Lumindex.Infrastructure.Search;

/// <summary>
/// Binds the <c>Azure:Search</c> configuration section. When <see cref="Endpoint"/> and
/// <see cref="ApiKey"/> are set the pipeline upserts into Azure AI Search; otherwise it writes a
/// local JSON index so retrieval can be developed/tested without a cloud search service.
/// </summary>
public sealed class AzureSearchOptions
{
    public const string SectionName = "Azure:Search";

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public string IndexName { get; set; } = "lumindex-chunks";

    /// <summary>Vector field dimensions; must match the embedding generator's output size.</summary>
    public int VectorDimensions { get; set; } = 1536;

    /// <summary>Root directory used by the local JSON fallback when no endpoint/key is configured.</summary>
    public string? LocalPath { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(ApiKey);
}
