using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using ShopNN.Services.Interface;
using System;
using System.Threading.Tasks;

namespace ShopNN.Services.Implement
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, string? toName = null)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new InvalidOperationException("SMTP Server config is missing.");
                var portStr = _configuration["EmailSettings:Port"] ?? "587";
                var port = int.Parse(portStr);
                var senderName = _configuration["EmailSettings:SenderName"] ?? "ShopNN";
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("Sender Email config is missing.");
                var username = _configuration["EmailSettings:Username"] ?? senderEmail;
                var password = _configuration["EmailSettings:Password"] ?? throw new InvalidOperationException("SMTP Password config is missing.");

                _logger.LogInformation("Sending email to {ToEmail} using server {SmtpServer}:{Port}...", toEmail, smtpServer, port);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(toName ?? toEmail, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // standard TLS port is 465, STARTTLS port is 587
                var secureSocketOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                
                await client.ConnectAsync(smtpServer, port, secureSocketOption);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} due to an error: {Message}", toEmail, ex.Message);
                throw;
            }
        }
    }
}
