using TaskMan.Models;

namespace TaskMan.ViewModels.Tasks
{
    public class DashboardViewModel
    {
        public List<UserTasksGrouped> StructuredUserTasks { get; set; }
    }

    public class UserTasksGrouped
    {
        public UserTasks UserTask { get; set; }
        public List<Models.Task> Tasks { get; set; }
    }
}
