using TaskMan.Models;

namespace TaskMan.ViewModels.Group
{
    public class GroupsViewModel
    {
        public List<GroupCard> GroupCards { get; set; }
    }

    public class GroupCard
    {
        public Models.Group Group { get; set; }
        public GroupUsers? Member { get; set; }
    }
}
