using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskMan.Models
{
    [PrimaryKey(nameof(User_ID), nameof(Group_List_ID))]
    public class GroupListTeam
    {
        [ForeignKey("User")]
        public string User_ID { get; set; }

        [ForeignKey("Group_List")]
        public int Group_List_ID { get; set; }
    }
}
