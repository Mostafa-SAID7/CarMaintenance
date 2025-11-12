using CommunityCar.Domain.Entities.Notifications;
using System.Linq.Expressions;

namespace CommunityCar.Domain.Specifications;

public class UserNotificationsSpecification : Specification<Notification>
{
    public UserNotificationsSpecification(string userId)
    {
        // Filter notifications for specific user
        AddCriteria(n => n.UserId == userId);

        // Order by creation date (newest first)
        AddOrderByDescending(n => n.CreatedAt);

        // Include related entities if needed
        // AddInclude(n => n.Sender);
    }

    public UserNotificationsSpecification(string userId, bool unreadOnly) : this(userId)
    {
        if (unreadOnly)
        {
            // Filter only unread notifications
            AddCriteria(n => !n.IsRead);
        }
    }

    public UserNotificationsSpecification(string userId, string type) : this(userId)
    {
        // Filter by notification type
        AddCriteria(n => n.Type == type);
    }

    public UserNotificationsSpecification(string userId, DateTime fromDate, DateTime? toDate = null) : this(userId)
    {
        // Filter by date range
        AddCriteria(n => n.CreatedAt >= fromDate);
        if (toDate.HasValue)
        {
            AddCriteria(n => n.CreatedAt <= toDate.Value);
        }
    }
}

public class UserNotificationsSpecification<T> : Specification<T> where T : class
{
    public UserNotificationsSpecification(Expression<Func<T, bool>> criteria)
    {
        AddCriteria(criteria);
        AddOrderByDescending(x => (DateTime)((dynamic)x).CreatedAt);
    }
}