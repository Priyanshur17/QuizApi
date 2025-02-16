using System.Net.Mail;
using System.Threading.Tasks;

namespace QuizAPI.Services
{
    public class EmailService
    {
        private readonly EmailConfig _emailConfig;

        public EmailService(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendMailAsync(string from, string to, string subject, string text, string html)
        {
            var smtpClient = _emailConfig.GetSmtpClient();

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = string.IsNullOrEmpty(html) ? text : html,
                IsBodyHtml = !string.IsNullOrEmpty(html)
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
