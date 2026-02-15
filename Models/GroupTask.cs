using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMan.Models
{
    public class GroupTask
    {
        [Key]
        public int Group_Task_ID { get; set; }

        [ForeignKey("Group_List")]
        public int Group_List_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly Due_Date { get; set; }
        public int Status { get; set; }

    }
}
