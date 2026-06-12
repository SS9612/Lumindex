namespace Lumindex.Infrastructure.Search;

/// <summary>
/// Field names for the chunk search index, shared between the indexer and (future) retrieval code so
/// the schema stays in one place.
/// </summary>
internal static class SearchIndexFields
{
    public const string Id = "id";
    public const string OwnerId = "ownerId";
    public const string DocumentId = "documentId";
    public const string Ordinal = "ordinal";
    public const string PageNumber = "pageNumber";
    public const string Content = "content";
    public const string ContentVector = "contentVector";

    public const string VectorProfile = "lumindex-vector-profile";
    public const string VectorAlgorithm = "lumindex-hnsw";
}
