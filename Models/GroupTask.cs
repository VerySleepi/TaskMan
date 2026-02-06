using System.ComponentModel.DataAnnotations;

namespace TaskMan.Models
{
    public class GroupTask
    {
        [Key]
        public int Group_Task_ID { get; set; }
        public int Group_List_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly Due_Date { get; set; }
        public int Status { get; set; }

    }
}
