using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;
public class TripModel : AuditModel
{
    public TripModel()
    {
        Id = "";
        OriginLatitude = "";
        OriginLongitude = "";
        DestinationLatitude = "";
        DestinationLongitude = "";
        PickupPointLatitude = "";
        PickupPointLongitude = "";
        PassengerId = "";
        PassengerName = "";
        Email = "";
        ContactNo = "";
        DriverId = "";
        DriverName = "";
        CountryName = "";
        CityId = "";
        CityName = "";
        VehicleFareId = "";
        VehicleFareName = "";
        RequestTime = DateTime.Now; //current date

        VehicleId = "";
        VehicleTypeId = "";
        VehicleTypeName = "";
        StatusId = "";
        StatusName = "";
		OriginAddress = "";
		DestinationAddress = "";
		CountryId = "";
        Message = "";
    }

    #region Address
    public string OriginLatitude { get; set; }
    public string OriginLongitude { get; set; }
    public string DestinationLatitude { get; set; }
    public string DestinationLongitude { get; set; }
    public string PickupPointLatitude { get; set; }
    public string PickupPointLongitude { get; set; }
    public string OriginAddress { get; set; }
    public string DestinationAddress { get; set; }

    public string Email { get; set; }
    public string ContactNo { get; set; }
    public string DriverName { get; set; }
    public string CityName { get; set; }
    public string CountryId { get; set; }
    public string CityId { get; set; }
    public string VehicleFareName { get; set; }
    public string CountryName { get; set; }
    public string VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; }
    public string Message { get; set; }
    #endregion

    #region Financial

    public double BaseFare { get; set; }
    public double CostPerKm { get; set; }
    public double EstimatedCost { get; set; }
    public double TaxAmount { get; set; }
    public double DistanceKm { get; set; }
    public double DurationMinute { get; set; }
    public double TotalCost { get; set; }

    public double CostPerMin { get; set; }
    public double InitialFee { get; set; }
    public double CostOfTravelTime { get; set; }
    public double CostOfDistance { get; set; }

    #endregion

    public string PassengerId { get; set; }
    public string DriverId { get; set; }
    public string TripInitialId { get; set; }
    public string VehicleId { get; set; }
    public string VehicleFareId { get; set; }
    public string PassengerName { get; set; }
    public string StatusId { get; set; }
	public string StatusName { get; set; }
	public string StartLocationName { get; set; }
    public double? InitialDistance { get; set; }
    public decimal? InitialRent { get; set; }
    public int EstimatedTime { get; set; }
    public double StartLocationLongitude { get; set; } = 0;
    public double StartLocationLatitude { get; set; } = 0;
    public string EndLocationName { get; set; }
    public double EndLocationLongitude { get; set; } = 0;
    public double EndLocationLatitude { get; set; } = 0;
    public DateTime RequestTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public decimal BaseAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime? PaymentTime { get; set; }
    public DateTime? PickupTimeFrom { get; set; }
    public DateTime? PickupTimeTo { get; set; }
}
