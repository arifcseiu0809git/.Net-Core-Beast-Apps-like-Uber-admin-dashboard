using BEASTAPI.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.ViewModel
{
    public class XDriverVehicleViewModel : AuditModel
    {
        public string UserId { get; set; }
        public string DriverName { get; set; }
        public string DriverLicenseNo { get; set; }
        public string VehicleId { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string VehicleType { get; set; }
        public string VehicleBrand { get; set; }
    }
}
