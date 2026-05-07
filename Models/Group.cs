using System.ComponentModel.DataAnnotations;

namespace TaskMan.Models
{
    public class Group
    {
        [Key]
        public int Group_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<GroupList> GroupList { get; set; }
        public List<GroupUsers> GroupUsers { get; set; }
    }
}
