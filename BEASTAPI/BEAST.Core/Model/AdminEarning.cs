using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model
{
    public class AdminEarning : AuditModel
    {
        public string TripId { get; set; }
        public bool IsCommisionReceived { get; set; }
        public DateTime CommissionReceiveDate { get; set; }
        public string TransactionId { get; set; }
        public decimal CommissionAmount { get; set; }
        public string PaymentTypeId { get; set; }
        public string PaymentOptionId { get; set; }
        public string PaymentMethodId { get; set; }

    }
}
