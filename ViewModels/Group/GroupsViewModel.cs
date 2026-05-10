using TaskMan.Models;

namespace TaskMan.ViewModels.Group
{
    public class GroupsViewModel
    {
        public List<Models.Group> Groups { get; set; }
        public List<GroupUsers> Members { get; set; }
    }
}
