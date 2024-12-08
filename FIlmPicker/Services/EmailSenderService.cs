using SamOtpravilEmailSender;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FIlmPicker.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly EmailSender _emailSender;

        public EmailSenderService(string smtpServer, int smtpPort, string login, string password, string senderName, string senderEmail)
        {
            _emailSender = new EmailSender(smtpServer, smtpPort, login, password, senderName, senderEmail);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
             await _emailSender.SendAsync(email, subject, htmlMessage);
        }
    }
}
