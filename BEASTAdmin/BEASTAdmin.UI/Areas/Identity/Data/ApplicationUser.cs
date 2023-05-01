using Microsoft.AspNetCore.Identity;

namespace BEASTAdmin.UI.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string UserType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }
}