using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Model
{
    public class CouponModel : AuditModel
    {
        public string UserId { get; set; }
        public string CouponCode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? DiscountOnFare { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public bool IsAppliedCoupon { get; set; }
    }
}
