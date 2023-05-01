using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model
{
    public class TransactionRequestModel : AuditModel
    {
        public string TripId { get; set; }
        public string APIEndPointRequestData { get; set; }
    }
}
