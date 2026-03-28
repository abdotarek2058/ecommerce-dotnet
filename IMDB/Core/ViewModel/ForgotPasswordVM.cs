using System.ComponentModel.DataAnnotations;

namespace IMDB.Core.ViewModel
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
