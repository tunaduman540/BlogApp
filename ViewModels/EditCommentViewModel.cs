using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class EditCommentViewModel
    {
        [Required(ErrorMessage = "Yorum boş bırakılamaz.")]
        public string Text { get; set; } = string.Empty;
    }
}
