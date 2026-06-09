using DocuMind.Domain.Entities;

namespace DocuMind.Application.Documents.Models;

/// <summary>Read model for a <see cref="Document"/> returned to API callers.</summary>
public sealed record DocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    string? StatusDetail,
    int ChunkCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt)
{
    public static DocumentDto FromEntity(Document document) => new(
        document.Id,
        document.FileName,
        document.ContentType,
        document.SizeBytes,
        document.Status.ToString(),
        document.StatusDetail,
        document.ChunkCount,
        document.CreatedAt,
        document.ProcessedAt);
}
