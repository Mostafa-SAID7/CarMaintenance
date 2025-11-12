using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the PostVote entity
/// </summary>
public class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
{
    /// <summary>
    /// Configures the PostVote entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<PostVote> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasOne(pv => pv.User)
            .WithMany()
            .HasForeignKey(pv => pv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pv => pv.Post)
            .WithMany(p => p.Votes)
            .HasForeignKey(pv => pv.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pv => new { pv.PostId, pv.UserId })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.UniqueIndexPattern,
                nameof(PostVote),
                "Post_User"))
            .IsUnique();

        builder.HasIndex(pv => pv.UserId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(PostVote),
                "UserId"));

        builder.HasIndex(pv => pv.PostId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(PostVote),
                "PostId"));

        builder.HasIndex(pv => pv.VoteType)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(PostVote),
                "VoteType"));
    }
}
