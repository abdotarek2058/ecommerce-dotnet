using System.ComponentModel.DataAnnotations;

namespace IMDB.Data.ViewModel
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
