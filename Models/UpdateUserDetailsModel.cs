namespace Cybage_Connect.Models
{
    public class UpdateUserDetailsModel
    {
        public string FullName { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string UserPassword { get; set; } = null!;

        public string? Email { get; set; }

        public long? MobileNumber { get; set; }
        public string? Designation { get; set; }
    }
}
