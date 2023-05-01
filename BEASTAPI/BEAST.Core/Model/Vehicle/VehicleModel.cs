using System.Xml.Linq;

namespace BEASTAPI.Core.Model.Vehicle
{
	public class VehicleModel : AuditModel
	{
		public string VehicleBrandId { get; set; }
		public string Name { get; set; }
		public int Year { get; set; }
		public int CubicCapacity { get; set; }
		public string VehicleBrandName { get; set; }
	}
}
