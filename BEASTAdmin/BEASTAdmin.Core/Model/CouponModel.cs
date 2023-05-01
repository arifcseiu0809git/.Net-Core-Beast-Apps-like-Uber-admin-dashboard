
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Model;

public class CouponModel : AuditModel
{
	public string UserId { get; set; }

    [Required(ErrorMessage = "Please enter 'Coupon Code'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Coupon Code' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Coupon Code' is 150 characters.")]
    public string CouponCode { get; set; }


	public DateTime? StartTime { get; set; }
	public DateTime? EndTime { get; set; }
	public decimal? DiscountOnFare { get; set; }
	public decimal? DiscountPercentage { get; set; }
	public bool IsAppliedCoupon { get; set; }

	public List<CouponModel> Coupons { get; } = new List<CouponModel>();
}

