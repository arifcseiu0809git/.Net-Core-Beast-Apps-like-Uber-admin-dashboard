using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model.Passenger;
public class PassengerRideHistoryModel : AuditModel
{
	public string PassengerName { get; set; }
	public string DriverName { get; set; }
	public string OriginAddress { get; set; }
	public string DestinationAddress { get; set; }
	public decimal TotalCost { get; set; }
	public string VehicleTypeName { get; set; }
	public string TripCancelledBy { get; set; }
	public DateTime? TripStartTime { get; set; }
	public DateTime? TripEndTime { get; set; }
	public DateTime? RequestTime { get; set; }
	public string VehicleBrandName { get; set; }
	public string VehicleModelName { get; set; }
	public string StatusName { get; set; }
}

