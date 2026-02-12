using Microsoft.AspNetCore.Identity;

namespace KidFit.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset TimeUpdated { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}
