using Lumindex.Domain.Entities;

namespace Lumindex.Application.Common.Persistence;

/// <summary>
/// Data access for <see cref="Conversation"/> aggregates and their <see cref="Message"/> children.
/// Every read is scoped by <c>ownerId</c> to enforce per-user data isolation.
/// </summary>
public interface IConversationRepository
{
    /// <summary>Lists the owner's conversations, most recently updated first (without messages).</summary>
    Task<IReadOnlyList<Conversation>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>Returns the owner's conversation, or <c>null</c> when missing or owned by someone else.</summary>
    Task<Conversation?> GetByIdAsync(Guid ownerId, Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>Returns the owner's conversation with its messages eagerly loaded in chronological order.</summary>
    Task<Conversation?> GetWithMessagesAsync(Guid ownerId, Guid conversationId, CancellationToken cancellationToken = default);

    void Add(Conversation conversation);

    void Remove(Conversation conversation);
}
