using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEASTAPI.Core.Model.Common
{
    public class SystemStatusModel : AuditModel
    {
        public string Name { get; set; }
    }
}
