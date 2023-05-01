using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.Coupon;

[Authorize(Roles = "SystemAdmin")]
public partial class CouponListModel : PageModel
{
    public List<CouponModel> Coupons { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<CouponListModel> _logger;
    private readonly CouponService _CouponService;

    public CouponListModel(ILogger<CouponListModel> logger, CouponService couponService)
    {
        this._logger = logger;
        this._CouponService = couponService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _CouponService.GetCoupons(PageNumber);
		Coupons = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
        return Page();
    });

    public Task<IActionResult> OnPostDelete(string id) =>
    TryCatch(async () =>
    {
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        await _CouponService.DeleteCoupon(id, logModel);
        return RedirectToPage("/Coupon/CouponList", new { c = "pmet", p = "pmetl" });
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _CouponService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}