namespace Cybage_Connect.Models
{
    public class FriendsListModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string? Email { get; set; }
        public string? Designation { get; set; }
    }
}
