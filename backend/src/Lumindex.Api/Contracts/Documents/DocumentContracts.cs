namespace Lumindex.Api.Contracts.Documents;

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    string? StatusDetail,
    int ChunkCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);
