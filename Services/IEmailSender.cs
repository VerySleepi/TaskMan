using Microsoft.AspNetCore.Identity.UI.Services;

namespace TaskMan.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string sender, string email, string subject, string message);
    }
}
