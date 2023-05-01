using System.ComponentModel.DataAnnotations;

namespace BEASTAPI.Core.Model.Vehicle
{
    public class Vehicle : AuditModel
    {
        public string VehicleBrandId { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public string RegistrationNo { get; set; }
		public DateTime RegistrationDate { get; set; }
        public string Color { get; set; }
        public string Fuel { get; set; }
        public int Seat { get; set; }
        public string EngineNo { get; set; }
        public string ChassisNo { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Laden { get; set; }
        public string Authority { get; set; }
        public string OwnerType { get; set; }
        public DateTime? FitnessExpiredOn { get; set; }
        public DateTime? InsuranceExpiresOn { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string VehicleTypeId { get; set; }
        public string StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
