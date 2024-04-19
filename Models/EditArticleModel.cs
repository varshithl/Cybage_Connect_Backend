namespace Cybage_Connect.Models
{
    public class EditArticleModel
    {
        public int ArticleId { get; set; }

        public string ArticleTitle { get; set; } = null!;
        public string? Content { get; set; }
    }
}
