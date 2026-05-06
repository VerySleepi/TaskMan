using Microsoft.AspNetCore.Mvc;

namespace TaskMan.ViewModels.Group
{
    public class NewGroupListViewModel
    {
        // Fields for new Task List
        [HiddenInput]
        public int Group_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public string BckGr_Col { get; set; }

        // The list of colours for the radio buttons
        public List<string> BackgroundColors { get; set;  }

    }
}
