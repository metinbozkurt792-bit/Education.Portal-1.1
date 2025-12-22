using Microsoft.AspNetCore.Identity;
using Education_Portal.Models;

namespace Education_Portal.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public DateTime? BanEndDate { get; set; }
        public string? ProfileImage { get; set; }
        public List<Enrollment> Enrollments { get; set; }

    }
}