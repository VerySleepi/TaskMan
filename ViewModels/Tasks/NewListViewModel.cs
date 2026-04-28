using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMan.ViewModels.Tasks
{
    public class NewListViewModel
    {
        // Fields for new Task List
        public string? User_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public string BckGr_Col { get; set; }

        // The list of colours for the radio buttons
        public List<string> BackgroundColors { get; } = new List<string>
        {
            "#E67E75", // Red
            "#8CDE8F", // Green
            "#59DAE6", // Blue
            "#F0EE7A", // Yellow
            "#DEB4F0", // Purple
        };
    }
}
