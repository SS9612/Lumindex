namespace Lumindex.Domain.Entities;

public enum MessageRole
{
    User = 0,
    Assistant = 1,
    System = 2,
}

public sealed class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = default!;
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
