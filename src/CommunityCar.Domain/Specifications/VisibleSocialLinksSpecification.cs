using CommunityCar.Domain.Entities.Auth;
using CommunityCar.Domain.Entities.Profile;
using System.Linq.Expressions;

namespace CommunityCar.Domain.Specifications;

public class VisibleSocialLinksSpecification : Specification<UserProfile>
{
    public VisibleSocialLinksSpecification()
    {
        // Only include profiles with visible social links
        AddCriteria(p => p.IsPublic && !string.IsNullOrEmpty(p.SocialLinks));

        // Include the related user
        AddInclude(p => p.User);

        // Order by user creation date
        AddOrderByDescending(p => p.User.CreatedAt);
    }

    public VisibleSocialLinksSpecification(string userId) : base()
    {
        // Specific user filter
        AddCriteria(p => p.UserId == userId);
    }

    public VisibleSocialLinksSpecification(bool includePrivate) : base()
    {
        if (!includePrivate)
        {
            // Only public profiles
            AddCriteria(p => p.IsPublic);
        }
        // If includePrivate is true, show all profiles with social links
        AddCriteria(p => !string.IsNullOrEmpty(p.SocialLinks));
    }
}

public class VisibleSocialLinksSpecification<T> : Specification<T> where T : class
{
    public VisibleSocialLinksSpecification(Expression<Func<T, bool>> criteria)
    {
        AddCriteria(criteria);
    }
}
