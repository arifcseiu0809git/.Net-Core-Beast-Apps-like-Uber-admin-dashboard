using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model
{
    public class TransactionResponseModel : AuditModel
    {
        public string TripId { get; set; }
        public string APIResponseData { get; set; }
    }
}
