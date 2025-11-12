using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the PostTag entity
/// </summary>
public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    /// <summary>
    /// Configures the PostTag entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pt => new { pt.PostId, pt.TagId })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.UniqueIndexPattern,
                nameof(PostTag),
                "Post_Tag"))
            .IsUnique();

        builder.HasIndex(pt => pt.PostId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(PostTag),
                "PostId"));

        builder.HasIndex(pt => pt.TagId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(PostTag),
                "TagId"));
    }
}
