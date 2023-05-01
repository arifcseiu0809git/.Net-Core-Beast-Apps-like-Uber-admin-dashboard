using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model.Vehicle;

public class VehicleBrandModel : AuditModel
{
    [DisplayName("Vehicle Brand Name")]
    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }
    public string Model { get; set; }

}