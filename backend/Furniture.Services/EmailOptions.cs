namespace Furniture.Services;

public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Sender address (required for Resend; SMTP From).</summary>
    public string FromEmail { get; set; } = "";

    public string FromName { get; set; } = "Furniture";

    /// <summary>Railway-friendly: HTTPS API, works on all Railway plans.</summary>
    public string? ResendApiKey { get; set; }

    /// <summary>Legacy / self-hosted. On Railway Hobby/Free, outbound SMTP is blocked.</summary>
    public string? SmtpHost { get; set; }

    public int SmtpPort { get; set; } = 587;

    public bool SmtpUseSsl { get; set; } = true;

    public string? SmtpUser { get; set; }

    public string? SmtpPassword { get; set; }

    public int TimeoutSeconds { get; set; } = 20;
}
