using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MarbleGraniteShop.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions emailOptions;

        public EmailSender(IOptions<EmailOptions> options)
        {
            emailOptions = options.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
           // return Execute(emailOptions.SendGridKey, subject, htmlMessage, email);
            return Execute(subject, htmlMessage, email);
        }
        //private Task Execute(string sendGridKEy, string subject, string message, string email)
        //{
        //    var client = new SendGridClient(sendGridKEy);
        //    var from = new EmailAddress("admin@bulky.com", "Bulky Books");
        //    var to = new EmailAddress(email, "End User");
        //    var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
        //    return client.SendEmailAsync(msg);
        //}
        private Task Execute(string subject, string message, string email)
        {
            MailMessage mailMessage = new MailMessage();
            MailAddress mailFromAddress = new MailAddress("MarbleGraniteShop@gmail.com");
            mailMessage.From = mailFromAddress;
            mailMessage.To.Add(email);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;

            SmtpClient emailClient = new SmtpClient();
            emailClient.Port = 587;
            emailClient.Host = "smtp.gmail.com"; //for gmail host  
            emailClient.EnableSsl = true;
            emailClient.UseDefaultCredentials = false;
            emailClient.Credentials = new NetworkCredential("MarbleGraniteShop@gmail.com", "Suresh@856#9829");
            emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return emailClient.SendMailAsync(mailMessage);
        }


    }
}

