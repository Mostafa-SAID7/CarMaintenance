using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Api.Models;

public class EmailMessage
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    public string? Cc { get; set; }

    public string? Bcc { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public string? From { get; set; }

    public string? ReplyTo { get; set; }

    public List<EmailAttachment>? Attachments { get; set; }

    public Dictionary<string, string>? Headers { get; set; }

    public int Priority { get; set; } = 0; // 0 = Normal, 1 = Low, 2 = High

    public DateTime? SendAt { get; set; } // For scheduled sending

    public string? TemplateId { get; set; }

    public Dictionary<string, object>? TemplateData { get; set; }

    public string? Category { get; set; }

    public List<string>? Tags { get; set; }

    public EmailMessage()
    {
        Attachments = new List<EmailAttachment>();
        Headers = new Dictionary<string, string>();
        TemplateData = new Dictionary<string, object>();
        Tags = new List<string>();
    }
}

public class EmailAttachment
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>();

    [Required]
    public string ContentType { get; set; } = string.Empty;

    public string? ContentId { get; set; } // For inline attachments
}

public class EmailTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public List<string> Placeholders { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EmailQueueItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public EmailMessage Message { get; set; } = new();
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProviderResponse { get; set; }
    public int Priority { get; set; }
}

public enum EmailStatus
{
    Pending,
    Sending,
    Sent,
    Failed,
    Cancelled
}

public class EmailProviderSettings
{
    public string Provider { get; set; } = string.Empty; // SMTP, SendGrid, Mailgun, etc.
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, string> AdditionalSettings { get; set; } = new();
}

public class EmailDeliveryReport
{
    public string MessageId { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public EmailStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProviderMessageId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class EmailCampaign
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EmailTemplate Template { get; set; } = new();
    public List<string> Recipients { get; set; } = new();
    public EmailCampaignStatus Status { get; set; } = EmailCampaignStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public EmailCampaignStats Stats { get; set; } = new();
}

public enum EmailCampaignStatus
{
    Draft,
    Scheduled,
    Sending,
    Sent,
    Cancelled
}

public class EmailCampaignStats
{
    public int TotalRecipients { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Opened { get; set; }
    public int Clicked { get; set; }
    public int Bounced { get; set; }
    public int Unsubscribed { get; set; }
    public double OpenRate => TotalRecipients > 0 ? (double)Opened / TotalRecipients * 100 : 0;
    public double ClickRate => TotalRecipients > 0 ? (double)Clicked / TotalRecipients * 100 : 0;
    public double BounceRate => TotalRecipients > 0 ? (double)Bounced / TotalRecipients * 100 : 0;
}