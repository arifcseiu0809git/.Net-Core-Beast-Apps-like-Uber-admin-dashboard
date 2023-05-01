using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Vehicle
{
    public class XDriverVehicleModel : AuditModel
    {
        public string UserId { get; set; }
        public string VehicleId { get; set; }
    }
}
