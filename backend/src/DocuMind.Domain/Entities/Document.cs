namespace DocuMind.Domain.Entities;

public enum DocumentStatus
{
    Pending = 0,
    Processing = 1,
    Ready = 2,
    Failed = 3,
}

public sealed class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string BlobPath { get; set; } = default!;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? StatusDetail { get; set; }
    public int ChunkCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}
