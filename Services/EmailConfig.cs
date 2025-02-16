using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace QuizAPI.Services
{
    public class EmailConfig
    {
        private readonly IConfiguration _configuration;

        public EmailConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SmtpClient GetSmtpClient()
        {
            return new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _configuration["Email:UserName"],
                    _configuration["Email:UserPass"]
                )
            };
        }
    }
}
