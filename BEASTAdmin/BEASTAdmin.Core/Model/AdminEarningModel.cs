using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model
{
    public class AdminEarningModel : AuditModel
    {
        public string TripId { get; set; }
        public bool IsCommisionReceived { get; set; }
        public DateTime CommissionReceiveDate { get; set; }
        public string CommissionPaymentMethodId { get; set; }
        public string TransactionId { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal TripFare { get; set; }
    }
}
