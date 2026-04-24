using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskMan.Models
{
    public class User : IdentityUser
    {
        public string Profile_Pic { get; set; } = "user.png";
        public int Complete_Tasks { get; set; } = 0;
        public DateOnly Creation_Date { get; set; }
        public bool Remembered { get; set; } = false;
    }
}
