using System.ComponentModel.DataAnnotations;

namespace TaskMan.Models
{
    public class User
    {
        [Key]
        public int User_ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Profile_Pic { get; set; }
        public int Complete_Tasks { get; set; }
        public DateOnly Creation_Date { get; set; }
        public int Two_Factor { get; set; }
    }
}
