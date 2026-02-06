using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TaskMan.Models
{
    [PrimaryKey(nameof(User_ID), nameof(Group_List_ID))]
    public class GroupListTeam
    {
        public int User_ID { get; set; }
        public int Group_List_ID { get; set; }
    }
}
