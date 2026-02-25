using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Diagnostics;

namespace TaskMan.Services
{
    public class EmailSender
    {
        public async Task SendEmailSender()
        {
            var message = new MimeMessage();

            var from = new MailboxAddress("TaskMan", "taskman@taskmanager.com");
            message.From.Add(from);
            var to = new MailboxAddress("Test", "George.O.Stead@student.shu.ac.uk");
            message.To.Add(to);
            message.Subject = "Lets test out emails!";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = """
                Hey,

                This is the test email for 2FA implementation using MailKit in Asp.Net.

                - TaskMan
                """
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("localhost", 1025);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            Debug.WriteLine("Mail sent!");

        }

    }
}
