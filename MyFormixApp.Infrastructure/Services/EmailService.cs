using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using MyFormixApp.Application.Services;

namespace MyFormixApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _client;
        private readonly string _from;

        public EmailService(IConfiguration configuration)
        {
            _client = new SmtpClient(configuration["Email:Host"])
            {
                Port = int.Parse(configuration["Email:Port"]),
                Credentials = new NetworkCredential(
                    configuration["Email:Username"],
                    configuration["Email:Password"]),
                EnableSsl = true,
            };
            _from = configuration["Email:From"];
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using var mailMessage = new MailMessage(_from, email, subject, message) { IsBodyHtml = true };
            await _client.SendMailAsync(mailMessage);
        }
    }
}
