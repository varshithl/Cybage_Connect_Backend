namespace Cybage_Connect.Models
{
    public class EditProjectModel
    {
        public int ProjectInsightId { get; set; }

        public string ProjectTitle { get; set; } = null!;

        public string? Tools { get; set; }

        public string? ProjectDescription { get; set; }

        public string? Duration { get; set; }
    }
}
