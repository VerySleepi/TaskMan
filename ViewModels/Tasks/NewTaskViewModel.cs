using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using TaskMan.Models;

namespace TaskMan.ViewModels.Tasks
{
    public class NewTaskViewModel
    {
        // Fields for new Task
        public int Task_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly Due_Date { get; set; }
        public int Status { get; set; }
        public int User_List_ID { get; set; }

        // The user's task lists (for the dropdown)
        [BindNever]
        public List<UserTasks>? UserTaskLists { get; set; }
    }
}
