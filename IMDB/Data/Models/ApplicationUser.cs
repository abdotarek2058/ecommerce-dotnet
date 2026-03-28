using Microsoft.AspNetCore.Identity;

namespace IMDB.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
