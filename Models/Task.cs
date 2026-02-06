using System.ComponentModel.DataAnnotations;

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
        public int User_List_ID { get; set; }
    }
}
