using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskMan.Models;

namespace TaskMan.ViewModels.Group
{
    public class NewGroupTaskViewModel
    {

        public int Group_Task_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly Due_Date { get; set; }
        public int Status { get; set; }
        public int Group_List_ID { get; set; }
        public int Group_ID { get; set; }

        [BindNever]
        public List<GroupList>? GroupLists { get; set; }

    }
}
