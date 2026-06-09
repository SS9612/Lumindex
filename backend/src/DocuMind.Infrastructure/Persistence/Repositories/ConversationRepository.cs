using DocuMind.Application.Common.Persistence;
using DocuMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Persistence.Repositories;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly DocuMindDbContext _db;

    public ConversationRepository(DocuMindDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Conversation>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        await _db.Conversations
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task<Conversation?> GetByIdAsync(Guid ownerId, Guid conversationId, CancellationToken cancellationToken = default) =>
        _db.Conversations
            .FirstOrDefaultAsync(c => c.OwnerId == ownerId && c.Id == conversationId, cancellationToken);

    public Task<Conversation?> GetWithMessagesAsync(Guid ownerId, Guid conversationId, CancellationToken cancellationToken = default) =>
        _db.Conversations
            .Where(c => c.OwnerId == ownerId && c.Id == conversationId)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(Conversation conversation) => _db.Conversations.Add(conversation);

    public void Remove(Conversation conversation) => _db.Conversations.Remove(conversation);
}
