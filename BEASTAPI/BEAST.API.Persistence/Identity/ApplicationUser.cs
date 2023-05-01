using Microsoft.AspNetCore.Identity;

namespace BEASTAPI.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string UserType { get; set; }
}