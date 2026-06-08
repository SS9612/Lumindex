namespace DocuMind.Domain.Entities;

public sealed class Chunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Guid OwnerId { get; set; }
    public int Ordinal { get; set; }
    public int? PageNumber { get; set; }
    public int TokenCount { get; set; }
    public string Content { get; set; } = default!;
}
