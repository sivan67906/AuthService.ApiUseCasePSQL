using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Services
{
    /// <summary>
    /// SMTP-based email service implementation using EmailSettings from configuration.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient (to) is required.", nameof(to));

            subject ??= string.Empty;
            body ??= string.Empty;

            // Respect cancellation early
            cancellationToken.ThrowIfCancellationRequested();

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = _settings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            try
            {
                _logger.LogInformation(
                    "Sending email via SMTP to {To}. Host: {Host}, Port: {Port}, EnableSsl: {EnableSsl}",
                    to, _settings.SmtpHost, _settings.SmtpPort, client.EnableSsl);

                // Note: SmtpClient.SendMailAsync does not accept a CancellationToken in many runtimes.
                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {To}.", to);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Email send cancelled to {To}.", to);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}.", to);
                throw;
            }
        }

        public async Task SendTwoFactorCodeAsync(string to, string code, CancellationToken cancellationToken = default)
        {
            var subject = "Your Two-Factor Authentication Code";
            var body = @$"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .code {{ font-size: 32px; font-weight: bold; color: #7367F0; letter-spacing: 4px;
                 background: #f8f8f8; padding: 15px 25px; border-radius: 8px; display: inline-block; }}
        .warning {{ color: #666; font-size: 14px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h2>Two-Factor Authentication</h2>
        <p>Use the following code to complete your sign-in:</p>
        <p class=""code"">{code}</p>
        <p class=""warning"">This code will expire in 5 minutes. If you didn't request this code, please ignore this email.</p>
    </div>
</body>
</html>";

            await SendAsync(to, subject, body, cancellationToken);
        }
    }
}