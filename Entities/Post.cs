
namespace BlogApp.Entities
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public DateTime PublishedOn { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "Pending";

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
