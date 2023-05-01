using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.ViewModel
{
    public class DriverCommissionModel
    {
        public string TripId { get; set; }
        public decimal TripFare { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime? CommissionReceiveDate { get; set; }
        public bool IsCommisionReceived { get; set; }
    }
}
