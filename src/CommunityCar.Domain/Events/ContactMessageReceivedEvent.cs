namespace CommunityCar.Domain.Events;

public class ContactMessageReceivedEvent
{
    public string ContactId { get; }
    public string Name { get; }
    public string Email { get; }
    public string Subject { get; }
    public string Message { get; }
    public string? PhoneNumber { get; }
    public string? Company { get; }
    public ContactPriority Priority { get; }
    public ContactCategory Category { get; }
    public Dictionary<string, string> AdditionalData { get; }
    public DateTime ReceivedAt { get; }

    public ContactMessageReceivedEvent(
        string contactId,
        string name,
        string email,
        string subject,
        string message,
        string? phoneNumber = null,
        string? company = null,
        ContactPriority priority = ContactPriority.Normal,
        ContactCategory category = ContactCategory.General,
        Dictionary<string, string>? additionalData = null)
    {
        ContactId = contactId;
        Name = name;
        Email = email;
        Subject = subject;
        Message = message;
        PhoneNumber = phoneNumber;
        Company = company;
        Priority = priority;
        Category = category;
        AdditionalData = additionalData ?? new Dictionary<string, string>();
        ReceivedAt = DateTime.UtcNow;
    }
}

public enum ContactPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum ContactCategory
{
    General = 1,
    Support = 2,
    Sales = 3,
    Partnership = 4,
    Feedback = 5,
    BugReport = 6,
    FeatureRequest = 7
}