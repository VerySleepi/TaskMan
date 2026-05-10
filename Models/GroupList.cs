using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMan.Models
{
    public class GroupList
    {
        [Key]
        public int Group_List_ID { get; set; }

        [ForeignKey("Group")]
        public int Group_ID { get; set; }

        public Group Group { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public string BckGr_Col { get; set; }
    }
}
