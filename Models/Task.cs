using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMan.Models
{
    public class Task
    {
        [Key]
        public int Task_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly Due_Date { get; set; }
        public int Status { get; set; }

        [ForeignKey("UserTasks")]
        public int User_List_ID { get; set; }
        public UserTasks UserTasks { get; set; }
    }
}
