using DocuMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocuMind.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName).HasMaxLength(512).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(256).IsRequired();
        builder.Property(d => d.BlobPath).HasMaxLength(1024).IsRequired();
        builder.Property(d => d.StatusDetail).HasMaxLength(2048);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        // Owned by an AppUser; deleting the user removes their documents.
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Primary access pattern: list a user's documents newest-first.
        builder.HasIndex(d => new { d.OwnerId, d.CreatedAt });
    }
}
