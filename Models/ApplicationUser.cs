using Microsoft.AspNetCore.Identity;

namespace SafeVaultAPI.Models
{
    // Extend IdentityUser for custom fields
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}