using DocuMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocuMind.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(32).IsRequired();

        // Ordered retrieval of a conversation's transcript.
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });
    }
}
