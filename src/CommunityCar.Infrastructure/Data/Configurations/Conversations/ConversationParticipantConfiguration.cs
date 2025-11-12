using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cp => new { cp.ConversationId, cp.UserId }).IsUnique();
        builder.HasIndex(cp => cp.ConversationId);
        builder.HasIndex(cp => cp.UserId);
        builder.HasIndex(cp => cp.Role);
        builder.HasIndex(cp => cp.Status);
        builder.HasIndex(cp => cp.LastReadAt);
    }
}
