using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model.Vehicle
{
	public class XDriverVehicleModel : AuditModel
	{
		[Display(Name = "Driver")]
		public string UserId { get; set; }
		public string DriverName { get; set; }
		public string DriverLicenseNo { get; set; }
		[Display(Name = "Vehicle")]
		public string VehicleId { get; set; }
		public string VehicleRegistrationNumber { get; set; }
		public string VehicleType { get; set; }
		public string VehicleBrand { get; set; }

	}
}
