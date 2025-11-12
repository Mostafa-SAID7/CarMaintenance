using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ModerationReportConfiguration : IEntityTypeConfiguration<ModerationReport>
{
    public void Configure(EntityTypeBuilder<ModerationReport> builder)
    {
        builder.Property(mr => mr.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(mr => mr.AdditionalInfo).HasMaxLength(500);
        builder.Property(mr => mr.ModeratorNotes).HasMaxLength(500);

        builder.HasOne(mr => mr.Reporter)
            .WithMany()
            .HasForeignKey(mr => mr.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.ReportedUser)
            .WithMany()
            .HasForeignKey(mr => mr.ReportedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Moderator)
            .WithMany()
            .HasForeignKey(mr => mr.ModeratorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(mr => mr.ReporterId);
        builder.HasIndex(mr => mr.ReportedUserId);
        builder.HasIndex(mr => mr.Status);
        builder.HasIndex(mr => mr.Type);
        builder.HasIndex(mr => mr.ContentType);
        builder.HasIndex(mr => mr.ContentId);
        builder.HasIndex(mr => mr.CreatedAt);
    }
}
