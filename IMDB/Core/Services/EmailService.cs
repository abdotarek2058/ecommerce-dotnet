using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using IMDB.Core.Interfaces;

namespace IMDB.Core.Services
{
    public class EmailService : IEmailService
    {

        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("Sending email to {Email}", email);
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject is required");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required");

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail));

            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;

            emailMessage.Body = new BodyBuilder
            {
                HtmlBody = message
            }.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                await client.SendAsync(emailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw new ApplicationException("Failed to send email", ex);
                
            }
            finally
            {
                await client.DisconnectAsync(true);

            }
            
            
        }
    }
    
}
