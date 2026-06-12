namespace Lumindex.Infrastructure.Storage;

/// <summary>
/// Binds the <c>Azure:Storage</c> configuration section. When <see cref="ConnectionString"/> is
/// set the app talks to Azure Blob Storage; otherwise it falls back to <see cref="LocalPath"/> on
/// the local file system so the ingestion pipeline runs without any cloud dependency.
/// </summary>
public sealed class AzureStorageOptions
{
    public const string SectionName = "Azure:Storage";

    public string? ConnectionString { get; set; }

    public string ContainerName { get; set; } = "documents";

    /// <summary>Root directory used by the local file-system fallback when no connection string is set.</summary>
    public string? LocalPath { get; set; }
}
