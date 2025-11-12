using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.HasOne(gm => gm.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.User)
            .WithMany()
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.InvitedBy)
            .WithMany()
            .HasForeignKey(gm => gm.InvitedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();
        builder.HasIndex(gm => gm.GroupId);
        builder.HasIndex(gm => gm.UserId);
        builder.HasIndex(gm => gm.Role);
        builder.HasIndex(gm => gm.Status);
    }
}
