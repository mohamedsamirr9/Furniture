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
                Credentials = new NetworkCredential("furnitureee23@gmail.com", "uecr tgzh tdmu folg"),
                EnableSsl = true
            };

            var mail = new MailMessage("furnitureee23@gmail.com", to, subject, body);

            await client.SendMailAsync(mail);
        }
    }
}
