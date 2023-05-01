using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model
{
    public class EmailSendingModel : AuditModel
    {
        public string UserId { get; set; }
        public string EmailBody { get; set; }
        public bool? IsSuccessfullySend { get; set; }
        public string FailReason { get; set; }
    }
}
