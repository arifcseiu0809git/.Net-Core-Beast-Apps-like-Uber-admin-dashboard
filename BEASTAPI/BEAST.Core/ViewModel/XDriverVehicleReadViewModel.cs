using BEASTAPI.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BEASTAPI.Core.ViewModel
{
	public class XDriverVehicleReadViewModel : AuditModel
	{
		public string UserId { get; set; }
		public string DriverNameWithLicenseNo { get; set; }
		public string VehicleId { get; set; }
		public string VehicleRegistrationNumberWithVehicleType { get; set; }
	}
}
