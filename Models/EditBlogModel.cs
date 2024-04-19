namespace Cybage_Connect.Models
{
    public class EditBlogModel
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; } = null!;

        public string? Content { get; set; }
    }
}
