using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class EventAttendeeConfiguration : IEntityTypeConfiguration<EventAttendee>
{
    public void Configure(EntityTypeBuilder<EventAttendee> builder)
    {
        builder.HasOne(ea => ea.Event)
            .WithMany(ge => ge.Attendees)
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ea => ea.User)
            .WithMany()
            .HasForeignKey(ea => ea.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ea => new { ea.EventId, ea.UserId }).IsUnique();
        builder.HasIndex(ea => ea.EventId);
        builder.HasIndex(ea => ea.UserId);
        builder.HasIndex(ea => ea.Status);
    }
}
