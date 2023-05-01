using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model
{
    public class TransactionModel : AuditModel
    {
        public string TripId { get; set; }
        public decimal TotalBillAmount { get; set; }
        public DateTime BillDate { get; set; }
    }
}
