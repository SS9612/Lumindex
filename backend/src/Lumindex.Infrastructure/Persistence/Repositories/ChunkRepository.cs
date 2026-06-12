using Lumindex.Application.Common.Persistence;
using Lumindex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumindex.Infrastructure.Persistence.Repositories;

public sealed class ChunkRepository : IChunkRepository
{
    private readonly LumindexDbContext _db;

    public ChunkRepository(LumindexDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Chunk>> ListByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default) =>
        await _db.Chunks
            .Where(c => c.OwnerId == ownerId && c.DocumentId == documentId)
            .OrderBy(c => c.Ordinal)
            .ToListAsync(cancellationToken);

    public void AddRange(IEnumerable<Chunk> chunks) => _db.Chunks.AddRange(chunks);

    public async Task RemoveByDocumentAsync(Guid ownerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var chunks = await _db.Chunks
            .Where(c => c.OwnerId == ownerId && c.DocumentId == documentId)
            .ToListAsync(cancellationToken);

        _db.Chunks.RemoveRange(chunks);
    }
}
