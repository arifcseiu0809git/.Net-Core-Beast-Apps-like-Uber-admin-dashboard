using BEASTAPI.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Passenger
{
    public class PassengersLocationModel : AuditModel
    {
        public int PassengerId { get; set; }
        public string LandmarkName { get; set; }
        public string LandmarkType { get; set; }
        public string ZipCode { get; set; }
        public string MapLatitude { get; set; }
        public string MapLongitude { get; set; }
        public string LandmarkTypeIcon { get; set; }
        public int SortingPriority { get; set; }

    }
}
