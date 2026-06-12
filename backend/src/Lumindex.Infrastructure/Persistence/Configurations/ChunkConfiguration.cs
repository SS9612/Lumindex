using Lumindex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumindex.Infrastructure.Persistence.Configurations;

public sealed class ChunkConfiguration : IEntityTypeConfiguration<Chunk>
{
    public void Configure(EntityTypeBuilder<Chunk> builder)
    {
        builder.ToTable("Chunks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).IsRequired();

        // Deleting a document removes its chunks.
        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // OwnerId is denormalized onto the chunk so per-user queries never need a join.
        builder.HasIndex(c => c.OwnerId);

        // A chunk's position within its document is unique and used for ordered retrieval.
        builder.HasIndex(c => new { c.DocumentId, c.Ordinal }).IsUnique();
    }
}
