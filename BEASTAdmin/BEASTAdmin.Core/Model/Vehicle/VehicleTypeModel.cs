using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model.Vehicle;

public class VehicleTypeModel : AuditModel
{
    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }

    [DataType(DataType.Currency)]
    [Range(0, int.MaxValue, ErrorMessage = "Unit Price can not be negative.")]
    public float UnitPricePerKm { get; set; }

    [DisplayName("Expiry Date")]
    [Required(ErrorMessage = "Please enter 'Expiry Date'.")]
    [DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(30);

    [DataType(DataType.Currency)]
    [Range(0, int.MaxValue, ErrorMessage = "Waiting Time Cost Per Min can not be negative.")]
    public float WaitingTimeCostPerMin { get; set; }
    public string ImageName { get; set; }
    public string ImageUrl { get; set; }

    [Required(ErrorMessage = "Please enter 'Description'.")]
    [MinLength(20, ErrorMessage = "Minimum length of 'Description' is 20 characters.")]
    [MaxLength(4000, ErrorMessage = "Maximum length of 'Description' is 4000 characters.")]
    public string Descriptions { get; set; }
    public int SortingPriority { get; set; } = 100;
}