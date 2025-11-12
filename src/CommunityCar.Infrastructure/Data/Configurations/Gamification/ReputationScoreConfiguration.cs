using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ReputationScoreConfiguration : IEntityTypeConfiguration<ReputationScore>
{
    public void Configure(EntityTypeBuilder<ReputationScore> builder)
    {
        builder.Property(rs => rs.TotalScore).HasDefaultValue(0);
        builder.Property(rs => rs.PostsScore).HasDefaultValue(0);
        builder.Property(rs => rs.CommentsScore).HasDefaultValue(0);
        builder.Property(rs => rs.VotesReceivedScore).HasDefaultValue(0);
        builder.Property(rs => rs.ModerationScore).HasDefaultValue(0);
        builder.Property(rs => rs.BadgesScore).HasDefaultValue(0);

        builder.HasOne(rs => rs.User)
            .WithOne()
            .HasForeignKey<ReputationScore>(rs => rs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rs => rs.UserId).IsUnique();
        builder.HasIndex(rs => rs.Level);
    }
}
