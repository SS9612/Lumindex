using System.Text.Json;
using Lumindex.Application.Common.Ingestion;

namespace Lumindex.Infrastructure.Search;

/// <summary>
/// Local-development fallback for <see cref="ISearchIndexer"/>. Persists each document's indexed
/// chunks (text + metadata + embedding) as a JSON file under
/// <c>{root}/{ownerId}/{documentId}.json</c>, mirroring the local blob-storage fallback. This keeps
/// the full ingestion pipeline runnable offline and leaves an inspectable index for retrieval work.
/// </summary>
public sealed class LocalFileSearchIndex : ISearchIndexer
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _root;

    public LocalFileSearchIndex(string rootPath)
    {
        _root = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task UpsertAsync(IReadOnlyList<IndexedChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        foreach (var group in chunks.GroupBy(c => (c.OwnerId, c.DocumentId)))
        {
            var path = ResolvePath(group.Key.OwnerId, group.Key.DocumentId);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var existing = await ReadAsync(path, cancellationToken);
            var merged = existing
                .Where(stored => group.All(c => c.ChunkId != stored.ChunkId))
                .Concat(group.Select(StoredChunk.From))
                .OrderBy(stored => stored.Ordinal)
                .ToList();

            await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, merged, SerializerOptions, cancellationToken);
        }
    }

    public Task DeleteByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(ownerId, documentId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private async Task<List<StoredChunk>> ReadAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new List<StoredChunk>();
        }

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var stored = await JsonSerializer.DeserializeAsync<List<StoredChunk>>(stream, SerializerOptions, cancellationToken);
        return stored ?? new List<StoredChunk>();
    }

    private string ResolvePath(Guid ownerId, Guid documentId) =>
        Path.Combine(_root, ownerId.ToString("D"), $"{documentId:D}.json");

    private sealed record StoredChunk(
        Guid ChunkId,
        Guid OwnerId,
        Guid DocumentId,
        int Ordinal,
        int? PageNumber,
        string Content,
        float[] Embedding)
    {
        public static StoredChunk From(IndexedChunk chunk) => new(
            chunk.ChunkId,
            chunk.OwnerId,
            chunk.DocumentId,
            chunk.Ordinal,
            chunk.PageNumber,
            chunk.Content,
            chunk.Embedding.ToArray());
    }
}
