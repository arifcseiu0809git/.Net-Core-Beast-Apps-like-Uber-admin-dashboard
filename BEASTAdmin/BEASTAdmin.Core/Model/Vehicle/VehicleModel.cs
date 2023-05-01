using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model.Vehicle;

public class VehicleModel : AuditModel
{

    [Required(ErrorMessage = "Please Select 'Model'.")]

    public string VehicleBrandId { get; set; }

    [DisplayName("Model Name")]
    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(1, ErrorMessage = "Minimum length of 'Name' is 1 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please enter 'Year'.")]
    [Range(1990, int.MaxValue, ErrorMessage = "Year value Should be Greater 1990.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Please enter 'CubicCapacity'.")]
    [Range(50, int.MaxValue, ErrorMessage = "Cubic Capacity Should be Greater 50 CC.")]
    public int CubicCapacity { get; set; }

    public string VehicleBrandName { get; set; }



}