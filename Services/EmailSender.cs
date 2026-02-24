using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace TaskMan.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // This would be changed to a more secure implementation in a production environment, however I will use
            // my university email and password for testing purposes, as I don't want to use my personal email for this project.
            var mail = "George.O.Stead@student.shu.ac.uk";
            var pw = "3t_66?L25p_!";

            var client = new SmtpClient("smtp.office365.com", 587)
            {
                Credentials = new NetworkCredential(mail, pw),
                EnableSsl = true
            };

            return client.SendMailAsync(
                new MailMessage(from: mail,
                to: email,
                subject,
                message));
        }
    }
}
