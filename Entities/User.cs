
namespace BlogApp.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
        
        public byte[]? ProfileImage { get; set; }

        public string? ProfileImageContentType { get; set; }

        public List<Post> Posts { get; set; } = new List<Post>();

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
