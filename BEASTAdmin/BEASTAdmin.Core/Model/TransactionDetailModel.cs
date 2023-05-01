using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model
{
    public class TransactionDetailModel : AuditModel
    {
        public string TransactionId { get; set; }
        public string PaymentMethodId { get; set; }
        public decimal TransactionAmount { get; set; }
        public string StatusId { get; set; }
    }
}
