using TaskMan.Models;

namespace TaskMan.ViewModels.Tasks
{
    public class UserTasksViewModel
    {
        public UserTasks UserTask { get; set; }
        public List<TaskMan.Models.Task> Tasks { get; set; }
    }
}
