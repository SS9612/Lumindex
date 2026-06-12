using Lumindex.Application.Common.Ingestion;
using Lumindex.Application.Common.Persistence;
using Lumindex.Application.Common.Storage;
using Lumindex.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lumindex.Application.Documents.Ingestion;

/// <summary>
/// Coordinates the document ingestion pipeline using only application abstractions, keeping the
/// orchestration logic provider-agnostic and unit-testable. Concrete extraction, embedding, and
/// indexing strategies are supplied by the infrastructure layer.
/// </summary>
public sealed class DocumentIngestionService : IDocumentIngestionService
{
    // Status detail is persisted to a column capped at 2048 chars (see DocumentConfiguration).
    private const int MaxStatusDetailLength = 2048;

    private readonly IDocumentRepository _documents;
    private readonly IChunkRepository _chunks;
    private readonly IBlobStorage _blobStorage;
    private readonly ITextExtractor _extractor;
    private readonly ITextChunker _chunker;
    private readonly IEmbeddingGenerator _embeddings;
    private readonly ISearchIndexer _searchIndexer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentIngestionService> _logger;

    public DocumentIngestionService(
        IDocumentRepository documents,
        IChunkRepository chunks,
        IBlobStorage blobStorage,
        ITextExtractor extractor,
        ITextChunker chunker,
        IEmbeddingGenerator embeddings,
        ISearchIndexer searchIndexer,
        IUnitOfWork unitOfWork,
        ILogger<DocumentIngestionService> logger)
    {
        _documents = documents;
        _chunks = chunks;
        _blobStorage = blobStorage;
        _extractor = extractor;
        _chunker = chunker;
        _embeddings = embeddings;
        _searchIndexer = searchIndexer;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task IngestAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(ownerId, documentId, cancellationToken);
        if (document is null)
        {
            // The document may have been deleted between enqueue and execution; nothing to do.
            _logger.LogWarning(
                "Skipping ingestion for missing document {DocumentId} (owner {OwnerId})",
                documentId,
                ownerId);
            return;
        }

        try
        {
            await MarkProcessingAsync(document, cancellationToken);

            // Re-indexing safety: clear any chunks/vectors from a previous run before writing fresh ones.
            await _chunks.RemoveByDocumentAsync(ownerId, documentId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _searchIndexer.DeleteByDocumentAsync(ownerId, documentId, cancellationToken);

            var pages = await ExtractAsync(document, cancellationToken);
            var textChunks = _chunker.Chunk(pages);

            if (textChunks.Count == 0)
            {
                _logger.LogWarning(
                    "Document {DocumentId} produced no chunks; marking ready with empty content",
                    documentId);
                await MarkReadyAsync(document, chunkCount: 0, "No extractable text found.", cancellationToken);
                return;
            }

            var entities = textChunks
                .Select(c => new Chunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    OwnerId = ownerId,
                    Ordinal = c.Ordinal,
                    PageNumber = c.PageNumber,
                    TokenCount = c.TokenCount,
                    Content = c.Content,
                })
                .ToList();

            _chunks.AddRange(entities);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var vectors = await _embeddings.GenerateAsync(
                entities.Select(e => e.Content).ToList(),
                cancellationToken);

            if (vectors.Count != entities.Count)
            {
                throw new InvalidOperationException(
                    $"Embedding count ({vectors.Count}) does not match chunk count ({entities.Count}).");
            }

            var indexed = entities
                .Zip(vectors, (entity, vector) => new IndexedChunk(
                    entity.Id,
                    ownerId,
                    documentId,
                    entity.Ordinal,
                    entity.PageNumber,
                    entity.Content,
                    vector))
                .ToList();

            await _searchIndexer.UpsertAsync(indexed, cancellationToken);

            await MarkReadyAsync(document, entities.Count, statusDetail: null, cancellationToken);

            _logger.LogInformation(
                "Ingested document {DocumentId} for owner {OwnerId}: {ChunkCount} chunks indexed",
                documentId,
                ownerId,
                entities.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Ingestion failed for document {DocumentId} (owner {OwnerId})", documentId, ownerId);
            await TryMarkFailedAsync(document, ex.Message, cancellationToken);
            throw; // surface to Hangfire so the failure is recorded and eligible for retry
        }
    }

    private async Task<IReadOnlyList<DocumentPage>> ExtractAsync(Document document, CancellationToken cancellationToken)
    {
        await using var stream = await _blobStorage.OpenReadAsync(document.BlobPath, cancellationToken);
        return await _extractor.ExtractAsync(stream, document.FileName, document.ContentType, cancellationToken);
    }

    private async Task MarkProcessingAsync(Document document, CancellationToken cancellationToken)
    {
        document.Status = DocumentStatus.Processing;
        document.StatusDetail = null;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkReadyAsync(Document document, int chunkCount, string? statusDetail, CancellationToken cancellationToken)
    {
        document.Status = DocumentStatus.Ready;
        document.ChunkCount = chunkCount;
        document.ProcessedAt = DateTimeOffset.UtcNow;
        document.StatusDetail = Truncate(statusDetail);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task TryMarkFailedAsync(Document document, string message, CancellationToken cancellationToken)
    {
        try
        {
            document.Status = DocumentStatus.Failed;
            document.StatusDetail = Truncate(message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception saveEx)
        {
            // Don't mask the original ingestion failure if the status update itself fails.
            _logger.LogError(saveEx, "Failed to persist Failed status for document {DocumentId}", document.Id);
        }
    }

    private static string? Truncate(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= MaxStatusDetailLength ? value : value[..MaxStatusDetailLength];
    }
}
