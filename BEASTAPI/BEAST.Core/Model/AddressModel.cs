using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model
{
    public class AddressModel : AuditModel
    { 
        public string UserId { get; set; }
        public string City { get; set; }
        public int? Zip { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }  
    }
}
