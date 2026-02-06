using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TaskMan.Models
{
    [PrimaryKey(nameof(User_ID), nameof(Group_ID))]
    public class GroupUsers
    {
        public int User_ID { get; set; }
        public int Group_ID { get; set; }
        public string Role { get; set; }
        public DateOnly Join_Date { get; set; }
    }
}
