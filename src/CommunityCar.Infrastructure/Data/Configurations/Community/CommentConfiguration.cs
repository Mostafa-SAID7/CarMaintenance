using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the Comment entity
/// </summary>
public class CommentConfiguration : BaseEntityConfiguration<Comment>
{
    /// <summary>
    /// Configures the Comment entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure required content property
        ConfigureRequiredStringProperty(
            builder,
            c => c.Content,
            5000, // Reasonable comment length
            EntityConfigurationConstants.ContentProperty);

        // Configure optional moderation reason
        ConfigureOptionalStringProperty(
            builder,
            c => c.ModerationReason,
            EntityConfigurationConstants.LongStringLength);

        // Configure relationships
        ConfigureRestrictRelationship(
            builder,
            c => c.Author,
            c => c.AuthorId,
            "Author");

        ConfigureRequiredRelationship(
            builder,
            c => c.Post,
            c => c.PostId,
            "Post");

        ConfigureSetNullRelationship(
            builder,
            c => c.ParentComment,
            c => c.ParentCommentId,
            "ParentComment");

        // Configure boolean properties with indexes
        ConfigureBooleanPropertyWithIndex(
            builder,
            c => c.IsAcceptedAnswer,
            "IsAcceptedAnswer");

        ConfigureBooleanPropertyWithIndex(
            builder,
            c => c.IsApproved,
            "IsApproved");

        // Configure composite indexes for common queries
        builder.HasIndex(c => new { c.PostId, c.IsApproved, c.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Comment),
                "Post_Approved_CreatedAt"));

        builder.HasIndex(c => new { c.AuthorId, c.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Comment),
                "Author_CreatedAt"));

        builder.HasIndex(c => new { c.ParentCommentId, c.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Comment),
                "Parent_CreatedAt"));

        // Configure index for thread hierarchy queries
        builder.HasIndex(c => new { c.PostId, c.ParentCommentId, c.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Comment),
                "Thread_Hierarchy"));
    }
}
