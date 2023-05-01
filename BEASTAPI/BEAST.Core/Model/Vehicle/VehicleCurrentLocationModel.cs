using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BEASTAPI.Core.Model.Vehicle
{
    public class VehicleCurrentLocationModel : AuditModel
    {
        public string VehicleId { get; set; }
       // public string VehicleBrandId { get; set; }
       // public string VehicleBrandName { get; set; }
       // public string VehicleModelId { get; set; }
       // public string VehicleModelName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }   
        public double PreviousLatitude { get; set; }
        public double PreviousLongitude { get; set; }   
        public string GoingDirection { get; set; }   
        public int? GoingDegree { get; set; }
		[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = true)]
		public DateTime LastUpdateAt { get; set; } //= DateTime.Now.AddDays(30);
		public bool IsOnline { get; set; }
	}
}