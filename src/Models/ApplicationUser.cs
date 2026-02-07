using KidFit.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace KidFit.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public DateTimeOffset TimeCreated { get; set; }
        public DateTimeOffset TimeUpdated { get; set; }
        public bool IsActive { get; set; } = true;
        public Role Role { get; set; } = Role.TEACHER;
    }
}
