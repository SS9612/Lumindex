using Lumindex.Application.Common.Persistence;
using Lumindex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumindex.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly LumindexDbContext _db;

    public DocumentRepository(LumindexDbContext db)
    {
        _db = db;
    }

    public Task<Document?> GetByIdAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default) =>
        _db.Documents
            .FirstOrDefaultAsync(d => d.OwnerId == ownerId && d.Id == documentId, cancellationToken);

    public async Task<IReadOnlyList<Document>> ListAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // The owner filter runs in SQL; ordering happens in memory because SQLite cannot ORDER BY a
        // DateTimeOffset. A single user's document set is small, so this is negligible and stays
        // provider-agnostic (works identically against Postgres in production).
        var documents = await _db.Documents
            .Where(d => d.OwnerId == ownerId)
            .ToListAsync(cancellationToken);

        return documents
            .OrderByDescending(d => d.CreatedAt)
            .ToList();
    }

    public Task<bool> ExistsAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default) =>
        _db.Documents.AnyAsync(d => d.OwnerId == ownerId && d.Id == documentId, cancellationToken);

    public void Add(Document document) => _db.Documents.Add(document);

    public void Remove(Document document) => _db.Documents.Remove(document);
}
