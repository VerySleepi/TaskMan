using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TaskMan.ViewModels.Account
{
    public class TwoFactorAuthenticationViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }

    }
}
