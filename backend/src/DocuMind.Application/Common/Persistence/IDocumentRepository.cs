using DocuMind.Domain.Entities;

namespace DocuMind.Application.Common.Persistence;

/// <summary>
/// Data access for <see cref="Document"/> aggregates. Every read is scoped by <c>ownerId</c>
/// so that one user can never observe or mutate another user's documents.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>Returns the owner's document, or <c>null</c> when it does not exist or belongs to someone else.</summary>
    Task<Document?> GetByIdAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>Lists the owner's documents, newest first.</summary>
    Task<IReadOnlyList<Document>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default);

    void Add(Document document);

    void Remove(Document document);
}
