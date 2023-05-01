using BEASTAPI.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model.Passenger
{
    public class PassengersPaymentMethodModel : AuditModel
    {
        public int PassengerId { get; set; }
        public string PaymentMethodName { get; set; }
        public string PaymentMethodType { get; set; }
        public string Password { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public int? ExpiredOnMonth { get; set; }
        public int? ExpiredOnYear { get; set; }
        public string Cvv { get; set; }
        public string CountryName { get; set; }
        public int SortingPriority { get; set; }
        public string PaymentMethodTypeIcon { get; set; }

    }
}
