using TaskMan.Models;

namespace TaskMan.ViewModels.Group
{
    public class GroupDetailViewModel
    {
        public TaskMan.Models.Group Group { get; set; }
        public List<GroupList> GroupLists { get; set; }
        public Dictionary<int, List<GroupTask>> TasksByList { get; set; }
        public string UserRole { get; set; }
        public string User_ID { get; set; }

    }
}
