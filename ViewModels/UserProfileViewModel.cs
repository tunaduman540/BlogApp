
using BlogApp.Entities;

namespace BlogApp.ViewModels
{
    public class UserProfileViewModel
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<Post> Posts { get; set; } = new();

        public int UserId { get; set; }
    }
}
