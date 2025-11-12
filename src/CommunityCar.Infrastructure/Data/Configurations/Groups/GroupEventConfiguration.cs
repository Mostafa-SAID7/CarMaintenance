using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GroupEventConfiguration : IEntityTypeConfiguration<GroupEvent>
{
    public void Configure(EntityTypeBuilder<GroupEvent> builder)
    {
        builder.Property(ge => ge.Title).HasMaxLength(200).IsRequired();
        builder.Property(ge => ge.Description).HasMaxLength(1000);
        builder.Property(ge => ge.Location).HasMaxLength(200);
        builder.Property(ge => ge.VirtualLink).HasMaxLength(500);

        builder.HasOne(ge => ge.Group)
            .WithMany(g => g.Events)
            .HasForeignKey(ge => ge.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ge => ge.Organizer)
            .WithMany()
            .HasForeignKey(ge => ge.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ge => ge.GroupId);
        builder.HasIndex(ge => ge.OrganizerId);
        builder.HasIndex(ge => ge.StartDate);
        builder.HasIndex(ge => ge.EndDate);
        builder.HasIndex(ge => ge.Status);
    }
}
