using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Content { get; set; } = string.Empty;

        public List<int> SelectedTagIds { get; set; } = new();
    }
}