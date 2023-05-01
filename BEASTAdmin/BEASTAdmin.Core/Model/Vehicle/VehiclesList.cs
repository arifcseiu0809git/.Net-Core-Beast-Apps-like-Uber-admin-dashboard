using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model.Vehicle;
public class VehiclesList : AuditModel
{
	[DisplayName("Brand")]
	public string VehicleBrandId { get; set; }
	public string VehicleBrandName { get; set; }
	[DisplayName("Model")]
	public string VehicleModelId { get; set; }
	public string VehicleModelName { get; set; }
	[DisplayName("Type")]
	public string VehicleTypeId { get; set; }
	public string VehicleTypeName { get; set; }
	[DisplayName("Status")]
	public string StatusId { get; set; }
	public string StatusName { get; set; }
	[DisplayName("Registration No")]
	public string RegistrationNo { get; set; }
	[DisplayName("Registration Date")]
	[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
	public DateTime? RegistrationDate { get; set; } = DateTime.Now.AddDays(30);
	[DisplayName("Color")]
	public string Color { get; set; }
	[DisplayName("Fuel")]
	public string FuelTypeId { get; set; }
	public string Fuel { get; set; }
	[DisplayName("Seat")]
	public int Seat { get; set; }
	[DisplayName("Engine No")]
	public string EngineNo { get; set; }
	[DisplayName("Chassis No")]
	public string ChassisNo { get; set; }
	[DisplayName("Weight")]
	public decimal? Weight { get; set; }
	[DisplayName("Laden")]
	public decimal? Laden { get; set; }
	[DisplayName("Authority")]
	public string AuthorityId { get; set; }
	public string Authority { get; set; }
	[DisplayName("Owner Type")]
	public string OwnerTypeId { get; set; }
	public string OwnerType { get; set; }
	[DisplayName("Fitness Expires Date")]
	[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = false)]
	public DateTime? FitnessExpiredOn { get; set; }
	[DisplayName("Insurance Expires Date")]
	[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = false)]
	public DateTime? InsuranceExpiresOn { get; set; }
	[DisplayName("Approval Status?")]
	public bool IsApproved { get; set; }
	[DisplayName("Approved By")]
	public string ApprovedBy { get; set; }
	[DisplayName("Engine No")]
	[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = false)]
	public DateTime? ApprovedOn { get; set; } = DateTime.Now.AddDays(30);

}

