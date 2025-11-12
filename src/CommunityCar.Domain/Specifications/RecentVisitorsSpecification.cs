using CommunityCar.Domain.Entities.Auth;
using System.Linq.Expressions;

namespace CommunityCar.Domain.Specifications;

public class RecentVisitorsSpecification : Specification<User>
{
    public RecentVisitorsSpecification(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        // Users who have logged in recently
        AddCriteria(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value >= cutoffDate);

        // Include profile information
        AddInclude(u => u.UserProfile);

        // Order by last login (most recent first)
        AddOrderByDescending(u => u.LastLoginAt);

        // Limit results
        ApplyPaging(0, 50);
    }

    public RecentVisitorsSpecification(string userId, int days = 30) : this(days)
    {
        // Additional filter for specific user if needed
        AddCriteria(u => u.Id == userId);
    }
}

public class RecentVisitorsSpecification<T> : Specification<T> where T : class
{
    public RecentVisitorsSpecification(Expression<Func<T, bool>> criteria)
    {
        AddCriteria(criteria);
        AddOrderByDescending(x => (DateTime)((dynamic)x).LastLoginAt ?? DateTime.MinValue);
    }
}