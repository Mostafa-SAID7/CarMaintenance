using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the CommentVote entity
/// </summary>
public class CommentVoteConfiguration : IEntityTypeConfiguration<CommentVote>
{
    /// <summary>
    /// Configures the CommentVote entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<CommentVote> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasOne(cv => cv.User)
            .WithMany()
            .HasForeignKey(cv => cv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cv => cv.Comment)
            .WithMany(c => c.Votes)
            .HasForeignKey(cv => cv.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cv => new { cv.CommentId, cv.UserId })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.UniqueIndexPattern,
                nameof(CommentVote),
                "Comment_User"))
            .IsUnique();

        builder.HasIndex(cv => cv.UserId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(CommentVote),
                "UserId"));

        builder.HasIndex(cv => cv.CommentId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(CommentVote),
                "CommentId"));

        builder.HasIndex(cv => cv.VoteType)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(CommentVote),
                "VoteType"));
    }
}
