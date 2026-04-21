using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Diagnostics;

namespace TaskMan.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string sender, string email, string subject, string message)
        {
            try
            {
                var mailMessage = new MimeMessage();

                var from = new MailboxAddress("TaskMan", sender);
                mailMessage.From.Add(from);
                var to = new MailboxAddress("Receipient", email);
                mailMessage.To.Add(to);
                mailMessage.Subject = subject;
                mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("localhost", 1025);
                await smtp.SendAsync(mailMessage);
                await smtp.DisconnectAsync(true);
                Debug.WriteLine("Mail sent!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending email: {ex.Message}");
            }

        }

    }
}
