using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model
{
    /// <summary>
    /// This model is used for mark all the trip commisions as paid by given driver id and date range 
    /// and insert new entries into AdminEarning accordingly 
    /// </summary>
    public class AdminEarningInsertModel
    {
        public string DriverId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsCommisionReceived { get; set; }
        public DateTime CommissionReceiveDate { get; set; }
        public string TransactionId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal CommissionRate { get; set; }
        public string PaymentTypeId { get; set; }
        public string PaymentOptionId { get; set; }
        public string PaymentMethodId { get; set; }

    }
}
