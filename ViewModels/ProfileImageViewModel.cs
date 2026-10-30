using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.ViewModels
{
    public class ProfileImageViewModel
    {
        [Required(ErrorMessage = "Bir fotoğraf seçmelisiniz.")]
        public IFormFile? ProfileImage { get; set; }
    }
}
