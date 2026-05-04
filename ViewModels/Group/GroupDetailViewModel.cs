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
        public List<MemberDetails> Members { get; set; }
        public List<User> NonMembers { get; set; }
    }

    public class MemberDetails
    {
        public User User { get; set; }
        public string Role { get; set; }
    }
}
