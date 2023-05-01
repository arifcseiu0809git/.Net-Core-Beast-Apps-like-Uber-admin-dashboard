using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAdmin.Core.Model
{
    public class CityModel : AuditModel
    {
        public string Name { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }
}
