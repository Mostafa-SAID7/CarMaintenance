using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.Content).HasMaxLength(2000).IsRequired();
        builder.Property(m => m.MessageType).HasMaxLength(50);
        builder.Property(m => m.AttachmentUrl).HasMaxLength(500);
        builder.Property(m => m.ModerationReason).HasMaxLength(500);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ReplyToMessage)
            .WithMany(m => m.Replies)
            .HasForeignKey(m => m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.ReplyToMessageId);
        builder.HasIndex(m => m.IsApproved);
        builder.HasIndex(m => m.CreatedAt);
    }
}
