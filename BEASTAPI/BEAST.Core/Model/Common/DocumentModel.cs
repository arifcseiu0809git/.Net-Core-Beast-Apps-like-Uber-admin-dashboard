using BEASTAPI.Core.Enums;
using BEASTAPI.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BEASTAPI.Core.Model.Common
{
    public class DocumentModel : AuditModel
    {
        public string UserId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentUrl { get; set; }


        //public int ReferenceId { get; set; }
        //public DocumentClassType DocumentClassType { get; set; }
        public string Name { get; set; }
        //public long FileSize { get; set; }
        //public string FileType { get; set; }
        //public Byte[] FileData { get; set; }
    }
}
