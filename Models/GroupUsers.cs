using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskMan.Models
{
    [PrimaryKey(nameof(User_ID), nameof(Group_ID))]
    public class GroupUsers
    {
        [ForeignKey("User")]
        public string User_ID { get; set; }

        [ForeignKey("Group")]
        public int Group_ID { get; set; }
       
        public User User { get; set; }
        public Group Group { get; set; }
        public string Role { get; set; }
        public DateOnly Join_Date { get; set; }
    }
}
