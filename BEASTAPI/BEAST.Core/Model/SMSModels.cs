using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model
{
    public class SMSModels : AuditModel
    {
        public string UserId { get; set; }
        public string BodyText { get; set; }
        public bool? IsSuccessfullySend { get; set; }
        public string FailReason { get; set; }
    }
}
