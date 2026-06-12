using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Lumindex.Application.Common.Ingestion;

namespace Lumindex.Infrastructure.Search;

/// <summary>
/// Upserts chunk vectors into Azure AI Search. The index is created on demand and exposes
/// <c>ownerId</c>/<c>documentId</c> as filterable fields so retrieval (and deletes) stay scoped to a
/// single user — the foundation of Lumindex's per-user data isolation.
/// </summary>
public sealed class AzureAiSearchIndexer : ISearchIndexer
{
    // Azure AI Search accepts up to 1000 documents per indexing batch.
    private const int BatchSize = 500;

    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly string _indexName;
    private readonly int _vectorDimensions;
    private readonly SemaphoreSlim _ensureLock = new(1, 1);
    private bool _indexEnsured;

    public AzureAiSearchIndexer(AzureSearchOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ApiKey);

        var endpoint = new Uri(options.Endpoint);
        var credential = new AzureKeyCredential(options.ApiKey);

        _indexName = options.IndexName;
        _vectorDimensions = options.VectorDimensions;
        _indexClient = new SearchIndexClient(endpoint, credential);
        _searchClient = new SearchClient(endpoint, _indexName, credential);
    }

    public async Task UpsertAsync(IReadOnlyList<IndexedChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        await EnsureIndexAsync(cancellationToken);

        for (var offset = 0; offset < chunks.Count; offset += BatchSize)
        {
            var batch = chunks
                .Skip(offset)
                .Take(BatchSize)
                .Select(ToSearchDocument);

            await _searchClient.MergeOrUploadDocumentsAsync(batch, cancellationToken: cancellationToken);
        }
    }

    public async Task DeleteByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        await EnsureIndexAsync(cancellationToken);

        var filter = $"{SearchIndexFields.OwnerId} eq '{ownerId}' and {SearchIndexFields.DocumentId} eq '{documentId}'";

        while (true)
        {
            var options = new SearchOptions { Filter = filter, Size = 1000 };
            options.Select.Add(SearchIndexFields.Id);

            var response = await _searchClient.SearchAsync<SearchDocument>("*", options, cancellationToken);

            var ids = new List<string>();
            await foreach (var result in response.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue(SearchIndexFields.Id, out var id) && id is string key)
                {
                    ids.Add(key);
                }
            }

            if (ids.Count == 0)
            {
                return;
            }

            await _searchClient.DeleteDocumentsAsync(SearchIndexFields.Id, ids, cancellationToken: cancellationToken);

            if (ids.Count < 1000)
            {
                return;
            }
        }
    }

    private static SearchDocument ToSearchDocument(IndexedChunk chunk) => new()
    {
        [SearchIndexFields.Id] = chunk.ChunkId.ToString(),
        [SearchIndexFields.OwnerId] = chunk.OwnerId.ToString(),
        [SearchIndexFields.DocumentId] = chunk.DocumentId.ToString(),
        [SearchIndexFields.Ordinal] = chunk.Ordinal,
        [SearchIndexFields.PageNumber] = chunk.PageNumber,
        [SearchIndexFields.Content] = chunk.Content,
        [SearchIndexFields.ContentVector] = chunk.Embedding.ToArray(),
    };

    private async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        if (_indexEnsured)
        {
            return;
        }

        await _ensureLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexEnsured)
            {
                return;
            }

            var index = BuildIndex();
            await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
            _indexEnsured = true;
        }
        finally
        {
            _ensureLock.Release();
        }
    }

    private SearchIndex BuildIndex()
    {
        var fields = new List<SearchField>
        {
            new SimpleField(SearchIndexFields.Id, SearchFieldDataType.String) { IsKey = true },
            new SimpleField(SearchIndexFields.OwnerId, SearchFieldDataType.String) { IsFilterable = true },
            new SimpleField(SearchIndexFields.DocumentId, SearchFieldDataType.String) { IsFilterable = true },
            new SimpleField(SearchIndexFields.Ordinal, SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true },
            new SimpleField(SearchIndexFields.PageNumber, SearchFieldDataType.Int32) { IsFilterable = true },
            new SearchableField(SearchIndexFields.Content),
            new SearchField(SearchIndexFields.ContentVector, SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                IsSearchable = true,
                VectorSearchDimensions = _vectorDimensions,
                VectorSearchProfileName = SearchIndexFields.VectorProfile,
            },
        };

        return new SearchIndex(_indexName, fields)
        {
            VectorSearch = new VectorSearch
            {
                Profiles = { new VectorSearchProfile(SearchIndexFields.VectorProfile, SearchIndexFields.VectorAlgorithm) },
                Algorithms = { new HnswAlgorithmConfiguration(SearchIndexFields.VectorAlgorithm) },
            },
        };
    }
}
