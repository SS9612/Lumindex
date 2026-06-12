namespace Lumindex.Domain.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = "New conversation";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Message> Messages { get; set; } = new();
}
