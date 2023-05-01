using BEASTAPI.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Passenger
{
    public class PassengerModel : AuditModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [DisplayName("System Stauts")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a 'System Stauts'.")]
        public string SystemStatusId { get; set; }

        public string SystemStatusName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? EnrollDate { get; set; }
        public string NID { get; set; }
        public string Password { get; set; }
        public string City { get; set; }
        public float Ratings { get; set; }

        public string UserType { get; set; }
        public string OtpCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Int32 TimeInMinutes { get; set; }
    }
}
