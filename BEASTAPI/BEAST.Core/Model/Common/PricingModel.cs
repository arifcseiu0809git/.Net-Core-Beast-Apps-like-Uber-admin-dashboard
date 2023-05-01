using BEASTAPI.Core.Model.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BEASTAPI.Core.Model.Common
{
    public class PricingModel : AuditModel
    {
        public string VehicleTypeId { get; set; }
        public double BaseFare { get; set; }
        public double BookingFee { get; set; }
        public double CostPerMin { get; set; }
        public double CostPerKm { get; set; }
        public double MinCharge { get; set; }
        public double CancelFee { get; set; }
        public string CurrencyName { get; set; } = "BDT";
        public int DistrictId { get; set; } = 0;
    }
}
