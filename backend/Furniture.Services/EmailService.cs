using Furniture.Servises_Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("tasbeehmohamed540@gmail.com", "hchu nxgt cqcd kvsk"),
                EnableSsl = true
            };

            var mail = new MailMessage("tasbeehmohamed540@gmail.com", to, subject, body);

            await client.SendMailAsync(mail);
        }
    }
}
