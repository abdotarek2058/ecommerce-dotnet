using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IMDB.Core.ViewModel 
{
    public class ProfileVM
    {
        [Display(Name = "full Name")]
        [Required]
        public string FullName { get; set; }


        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}

