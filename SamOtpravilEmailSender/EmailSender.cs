using MailKit.Net.Smtp;
using MimeKit;

namespace SamOtpravilEmailSender
{
    public class EmailSender
    {
        private readonly string _smtpHost = "";
        private readonly int _smtpPort = 0;
        private readonly string _smtpLogin;
        private readonly string _smtpPassword;

        public EmailSender(string smtpLogin, string smtpPassword)
        {
            _smtpLogin = smtpLogin;
            _smtpPassword = smtpPassword;
        }

        public async Task SendAsync(string email, string subject, string body)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress("", ""));
            message.To.Add(new MailboxAddress("", email));

            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Connect(_smtpHost, _smtpPort);

                smtp.Authenticate(_smtpLogin, _smtpPassword);

                await smtp.SendAsync(message);

                smtp.Disconnect(true);
            }
        }
    }
}
