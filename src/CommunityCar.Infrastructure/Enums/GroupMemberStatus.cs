namespace CommunityCar.Infrastructure.Enums;

public enum GroupMemberStatus
{
    Pending = 1,    // Waiting for approval
    Active = 2,     // Active member
    Suspended = 3,  // Temporarily suspended
    Banned = 4,     // Permanently banned
    Left = 5        // Voluntarily left
}