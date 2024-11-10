using SamOtpravilEmailSender;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FIlmPicker.Services
{
    public class EmaiSenderService : IEmailSender
    {
        private readonly EmailSender _emailSender;

        public EmaiSenderService(string login, string password, string senderName, string senderEmail)
        {
            _emailSender = new EmailSender(login, password, senderName, senderEmail);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
             await _emailSender.SendAsync(email, subject, htmlMessage);
        }
    }
}
