
using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Yorum boş bırakılamaz.")]
        public string Text { get; set; } = string.Empty;
    }
}
