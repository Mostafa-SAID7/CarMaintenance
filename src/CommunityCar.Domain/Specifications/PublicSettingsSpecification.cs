using CommunityCar.Domain.Entities.Auth;
using System.Linq.Expressions;

namespace CommunityCar.Domain.Specifications;

public class PublicSettingsSpecification : Specification<User>
{
    public PublicSettingsSpecification()
    {
        // Include only users with public profiles
        AddCriteria(u => u.UserProfile != null && u.UserProfile.IsPublic);

        // Include related profile data
        AddInclude(u => u.UserProfile);

        // Order by creation date (newest first)
        AddOrderByDescending(u => u.CreatedAt);
    }

    public PublicSettingsSpecification(string userId) : base()
    {
        // Specific user filter
        AddCriteria(u => u.Id == userId);
    }
}

public class PublicSettingsSpecification<T> : Specification<T> where T : class
{
    public PublicSettingsSpecification(Expression<Func<T, bool>> criteria)
    {
        AddCriteria(criteria);
    }
}