using MailKit.Net.Smtp;
using MimeKit;

namespace SamOtpravilEmailSender
{
    public class EmailSender
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpLogin;
        private readonly string _smtpPassword;
        private readonly string _senderName;
        private readonly string _senderEmail;

        public EmailSender(string smtpHost, int smtpPort, string smtpLogin, string smtpPassword, string senderName, string senderEmail)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpLogin = smtpLogin;
            _smtpPassword = smtpPassword;
            _senderName = senderName;
            _senderEmail = senderEmail;
        }

        public async Task SendAsync(string email, string subject, string body)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To.Add(new MailboxAddress("", email));

            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Connect(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect);

                smtp.Authenticate(_smtpLogin, _smtpPassword);

                await smtp.SendAsync(message);

                smtp.Disconnect(true);
            }
        }
    }
}
