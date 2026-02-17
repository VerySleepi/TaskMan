using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMan.Models
{
    public class UserTasks
    {
        [Key]
        public int User_List_ID { get; set; }

        [ForeignKey("User")]
        public string User_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public string BckGr_Col { get; set; }
    }
}
