using BEASTAPI.Core.Model.Common;
using BEASTAPI.Core.Model.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Map
{
    public class TripInitialModel : AuditModel
    {
       
        public string OriginLatitude { get; set; }
        public string OriginLongitude { get; set; }
        public string DestinationLatitude { get; set; }
        public string DestinationLongitude { get; set; }
        public string PickupPointLatitude { get; set; }
        public string PickupPointLongitude { get; set; }

        #region Relational

        public string PassangerUserId { get; set; }
        public string PassengerId { get; set; }
        public string PassengerName { get; set; }
        public string ContactNo { get; set; }
		public string Email { get; set; }
		
		public string TripId { get; set; }
		public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string VehicleFareId { get; set; }
        public string VehicleFareName { get; set; }
        public string VehicleId { get; set; }
        public string VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; }
        public string StatusId { get; set; } //1 for Upcomming, 2 For Completed
        public string StatusName { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime RequestTime { get; set; }
        #endregion

        #region Not Mapped Properties

       public string ? PaymentMethodId { get; set; }
       public string ? PaymentTypeId { get; set; }
       public string ? PaymentOptionId { get; set; }
       public string ? AccountNumber { get; set; }
        public string? ExpireMonthYear { get; set; }
        public string? CvvCode { get; set; }

        

        #endregion

        #region From Matrix
        public string OriginAddress { get; set; }
        public string DestinationAddress { get; set; }
        public double DistanceValue { get; set; }
        public string DistanceText { get; set; }
        public double DurationValue { get; set; }
        public string DurationText { get; set; }
        public double DurationInTrafficValue { get; set; }
        public string DurationInTrafficText { get; set; }
        #endregion

        #region Financial

        public double BaseFare { get; set; }
        public double CostPerKm { get; set; }
        public double DistanceKm { get; set; }
        public double DurationMinute { get; set; }
        public double EstimatedCost { get; set; }
        public double TaxAmount { get; set; }
        public double TotalCost { get; set; }

           public double CostPerMin { get; set; }
        
        public double InitialFee { get; set; }
        public double CostOfTravelTime { get; set; }
        public double CostOfDistance { get; set; }
        
        #endregion
    }
}
