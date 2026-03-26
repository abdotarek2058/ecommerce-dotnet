using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;


namespace IMDB.Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
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

            emailMessage.Body = new TextPart("html")
            {
                Text = message
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                await client.SendAsync(emailMessage);
            }
            catch (Exception ex) 
            {
                throw new Exception("Failed to send email", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);

            }
            
            
        }
    }
    
}
