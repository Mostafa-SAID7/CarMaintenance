using CommunityCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.Property(cm => cm.Message).HasMaxLength(2000).IsRequired();
        builder.Property(cm => cm.MessageType).HasMaxLength(50);
        builder.Property(cm => cm.AttachmentUrl).HasMaxLength(500);
        builder.Property(cm => cm.ConversationId).HasMaxLength(100);

        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Receiver)
            .WithMany()
            .HasForeignKey(cm => cm.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.ReplyToMessage)
            .WithMany()
            .HasForeignKey(cm => cm.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(cm => cm.SenderId);
        builder.HasIndex(cm => cm.ReceiverId);
        builder.HasIndex(cm => cm.IsRead);
        builder.HasIndex(cm => cm.ConversationId);
        builder.HasIndex(cm => cm.CreatedAt);
        builder.HasIndex(cm => new { cm.SenderId, cm.ReceiverId });
    }
}
