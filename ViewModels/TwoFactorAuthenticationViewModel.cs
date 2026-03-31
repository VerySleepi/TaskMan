using System.ComponentModel.DataAnnotations;

namespace TaskMan.ViewModels
{
    public class TwoFactorAuthenticationViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
