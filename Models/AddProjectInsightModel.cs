namespace Cybage_Connect.Models
{
    public class AddProjectInsightModel
    {
        public string UserName { get; set; } = null!;

        public string ProjectTitle { get; set; } = null!;

        public string? Tools { get; set; }

        public string? ProjectDescription { get; set; }

        public string? Duration { get; set; }
    }
}
