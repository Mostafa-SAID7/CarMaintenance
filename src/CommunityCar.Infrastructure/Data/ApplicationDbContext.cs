using CommunityCar.Domain.Common;
using CommunityCar.Domain.Entities.Auth;
using CommunityCar.Domain.Entities.Car;
using CommunityCar.Domain.Entities.Booking;
using CommunityCar.Domain.Entities.Profile;
using CommunityCar.Domain.Entities;
using CommunityCar.Domain.Entities.Community;
using CommunityCar.Infrastructure.Data.Configurations.Auth;
using CommunityCar.Infrastructure.Data.Configurations.Core;
using CommunityCar.Infrastructure.Data.Configurations.Audit;
using CommunityCar.Infrastructure.Data.Configurations.Communication;
using CommunityCar.Infrastructure.Data.Configurations.Community;
using CommunityCar.Infrastructure.Data.Configurations.Groups;
using CommunityCar.Infrastructure.Data.Configurations.Conversations;
using CommunityCar.Infrastructure.Data.Configurations.Moderation;
using CommunityCar.Infrastructure.Data.Configurations.Gamification;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CommunityCar.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for domain entities
    public DbSet<Car> Cars { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    // Community DbSets
    public DbSet<Forum> Forums { get; set; }
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostVote> PostVotes { get; set; }
    public DbSet<CommentVote> CommentVotes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<GroupEvent> GroupEvents { get; set; }
    public DbSet<EventAttendee> EventAttendees { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ModerationReport> ModerationReports { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<ReputationScore> ReputationScores { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply global query filters for soft delete
        ApplyGlobalFilters(builder);

        // Apply entity configurations with improved namespace structure
        builder.ApplyConfiguration(new Auth.UserConfiguration());
        builder.ApplyConfiguration(new Auth.RoleConfiguration());
        builder.ApplyConfiguration(new Core.CarConfiguration());
        builder.ApplyConfiguration(new Core.BookingConfiguration());
        builder.ApplyConfiguration(new Auth.RefreshTokenConfiguration());
        builder.ApplyConfiguration(new Audit.AuditLogConfiguration());
        builder.ApplyConfiguration(new Core.UserProfileConfiguration());
        builder.ApplyConfiguration(new Auth.ApiKeyConfiguration());
        builder.ApplyConfiguration(new Communication.NotificationConfiguration());
        builder.ApplyConfiguration(new Communication.ChatMessageConfiguration());

        // Apply community entity configurations
        builder.ApplyConfiguration(new Community.ForumConfiguration());
        builder.ApplyConfiguration(new Community.ForumCategoryConfiguration());
        builder.ApplyConfiguration(new Community.PostConfiguration());
        builder.ApplyConfiguration(new Community.CommentConfiguration());
        builder.ApplyConfiguration(new Community.PostVoteConfiguration());
        builder.ApplyConfiguration(new Community.CommentVoteConfiguration());
        builder.ApplyConfiguration(new Community.TagConfiguration());
        builder.ApplyConfiguration(new Community.PostTagConfiguration());
        builder.ApplyConfiguration(new Groups.GroupConfiguration());
        builder.ApplyConfiguration(new Groups.GroupMemberConfiguration());
        builder.ApplyConfiguration(new Groups.GroupEventConfiguration());
        builder.ApplyConfiguration(new Groups.EventAttendeeConfiguration());
        builder.ApplyConfiguration(new Conversations.ConversationConfiguration());
        builder.ApplyConfiguration(new Conversations.ConversationParticipantConfiguration());
        builder.ApplyConfiguration(new Conversations.MessageConfiguration());
        builder.ApplyConfiguration(new Moderation.ModerationReportConfiguration());
        builder.ApplyConfiguration(new Gamification.BadgeConfiguration());
        builder.ApplyConfiguration(new Gamification.UserBadgeConfiguration());
        builder.ApplyConfiguration(new Gamification.ReputationScoreConfiguration());
    }

    private void ApplyGlobalFilters(ModelBuilder builder)
    {
        // Apply soft delete filter to all BaseEntity inheritors
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(GetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                var filter = method?.Invoke(this, Array.Empty<object>());
                if (filter != null)
                {
                    builder.Entity(entityType.ClrType).HasQueryFilter((LambdaExpression)filter);
                }
            }
        }
    }

    private LambdaExpression GetSoftDeleteFilter<TEntity>() where TEntity : BaseEntity
    {
        Expression<Func<TEntity, bool>> filter = e => !e.IsDeleted;
        return filter;
    }
}
