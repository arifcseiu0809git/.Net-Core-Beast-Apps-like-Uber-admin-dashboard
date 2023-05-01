using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BEASTAPI.Core.Model.Common
{
    public class MessageModel : AuditModel
    {
        public string TitleText { get; set; }
        public string DescriptionText { get; set; }
        public string IconName { get; set; }
        public string Param { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public string ExternalLink { get; set; }
        public bool IsSeen { get; set; }
        public DateTime? SeenTime { get; set; }
    }
}
