using BEASTAPI.Core.Enums;
using BEASTAPI.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BEASTAPI.Core.Model.Common
{
    public class DocumentTypeModel : AuditModel
    {
        public string Name { get; set; }
        public DocumentFor DocumentFor { get; set; }


	}
}
