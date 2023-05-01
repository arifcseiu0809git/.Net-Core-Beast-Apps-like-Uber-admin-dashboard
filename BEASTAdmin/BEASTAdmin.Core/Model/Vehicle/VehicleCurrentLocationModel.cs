using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model.Vehicle;
public class VehicleCurrentLocationModel : AuditModel
{
	[Required(ErrorMessage = "Please select an Option")]
	public string VehicleId { get; set; }
	//public string VehicleBrandId { get; set; }

	//[DisplayName("Brand")]
	//[Required(ErrorMessage = "Please Select a Brand")]
	//public string VehicleBrandName { get; set; }
	////public string VehicleModelId { get; set; }

	//[DisplayName("Model")]
	//[Required(ErrorMessage = "Please Select a Model")]
	//public string VehicleModelName { get; set; }
	[Required(ErrorMessage = "Please enter Latitude")]
	public double Latitude { get; set; }
	[Required(ErrorMessage = "Please enter Longitude")]
	public double Longitude { get; set; }
	public double PreviousLatitude { get; set; }
	public double PreviousLongitude { get; set; }
	[Required(ErrorMessage = "Please enter GoingDirection")]
	public string GoingDirection { get; set; }
	public int? GoingDegree { get; set; }
	[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime LastUpdateAt { get; set; } = DateTime.Now.AddDays(30);
	[DisplayName("Is Available")]
	public bool IsOnline { get; set; }

}