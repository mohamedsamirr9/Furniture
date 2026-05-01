using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json.Serialization;
using Furniture.Servises_Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Furniture.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_options.ResendApiKey))
            {
                await SendViaResendAsync(to, subject, body).ConfigureAwait(false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_options.SmtpHost))
            {
                await SendViaSmtpAsync(to, subject, body).ConfigureAwait(false);
                return;
            }

            _logger.LogWarning(
                "Email not sent: configure Email:ResendApiKey (recommended for Railway) or Email:Smtp* for SMTP.");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Email send timed out or was cancelled. To: {To}, Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed. To: {To}, Subject: {Subject}", to, subject);
        }
    }

    private async Task SendViaResendAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogWarning("Resend: FromEmail is not configured; skipping send.");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        var client = _httpClientFactory.CreateClient("Resend");
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new ResendEmailRequest
            {
                From = $"{_options.FromName} <{_options.FromEmail}>",
                To = [to],
                Subject = subject,
                Text = body
            })
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_options.ResendApiKey}");

        var response = await client.SendAsync(request, cts.Token).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
            _logger.LogWarning(
                "Resend returned {StatusCode}: {Detail}",
                (int)response.StatusCode,
                detail.Length > 500 ? detail[..500] : detail);
        }
    }

    private async Task SendViaSmtpAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogWarning("SMTP: FromEmail is not configured; skipping send.");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpUseSsl,
            Timeout = _options.TimeoutSeconds * 1000
        };

        if (!string.IsNullOrEmpty(_options.SmtpUser))
        {
            client.Credentials = new NetworkCredential(_options.SmtpUser, _options.SmtpPassword);
        }

        using var mail = new MailMessage(_options.FromEmail, to, subject, body);
        await client.SendMailAsync(mail, cts.Token).ConfigureAwait(false);
    }

    private sealed class ResendEmailRequest
    {
        [JsonPropertyName("from")]
        public required string From { get; init; }

        [JsonPropertyName("to")]
        public required string[] To { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("text")]
        public required string Text { get; init; }
    }
}
