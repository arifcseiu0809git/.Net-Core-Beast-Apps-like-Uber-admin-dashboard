using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.Coupon;

[Authorize(Roles = "SystemAdmin")]
public partial class CouponUpsertModel : PageModel
{
    [BindProperty]
    public CouponModel Coupon { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<CouponUpsertModel> _logger;
    private readonly CouponService _CouponService;

    public CouponUpsertModel(ILogger<CouponUpsertModel> logger, CouponService CouponService)
    {
        this._logger = logger;
        this._CouponService = CouponService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
			Coupon = new CouponModel();
        }
        else
        {
			Coupon = await _CouponService.GetCouponById(id);
        }

        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false) return Page();

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.1";

        if (string.IsNullOrEmpty(Coupon.Id))
        {
			Coupon.CreatedBy = User.Identity.Name;
			Coupon.IsActive = true;
			Coupon.IsDeleted = false;
            await _CouponService.InsertCoupon(Coupon, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
			Coupon.ModifiedBy = User.Identity.Name;
			Coupon.ModifiedDate = DateTime.Now;
            await _CouponService.UpdateCoupon(Coupon.Id, Coupon, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        return Page();
    });
}