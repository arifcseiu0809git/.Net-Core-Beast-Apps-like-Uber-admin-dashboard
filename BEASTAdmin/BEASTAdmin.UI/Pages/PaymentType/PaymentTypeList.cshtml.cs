using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.PaymentType;

[Authorize(Roles = "SystemAdmin")]
public partial class PaymentTypeListModel : PageModel
{
    public List<PaymentTypeModel> paymentTypes { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<PaymentTypeListModel> _logger;
    private readonly PaymentTypeService _paymentTypeService;

    public PaymentTypeListModel(ILogger<PaymentTypeListModel> logger, PaymentTypeService paymentTypeService)
    {
        this._logger = logger;
        this._paymentTypeService = paymentTypeService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _paymentTypeService.GetPaymentTypes(PageNumber);
		paymentTypes = paginatedList.Items;
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

        await _paymentTypeService.DeletePaymentType(id, logModel);
        return RedirectToPage("/PaymentType/PaymentTypeList", new { c = "pmet", p = "pmetl" });
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _paymentTypeService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}