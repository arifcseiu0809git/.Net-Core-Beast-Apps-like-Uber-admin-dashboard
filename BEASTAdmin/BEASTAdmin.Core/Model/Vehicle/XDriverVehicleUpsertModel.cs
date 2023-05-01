using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model.Vehicle
{
	public class XDriverVehicleUpsertModel : AuditModel
	{
		[Display(Name = "Driver")]
		public string UserId { get; set; }
		public string DriverNameWithLicenseNo { get; set; }
		[Display(Name = "Vehicle")]
		public string VehicleId { get; set; }
		public string VehicleRegistrationNumberWithVehicleType { get; set; }

	}
}
