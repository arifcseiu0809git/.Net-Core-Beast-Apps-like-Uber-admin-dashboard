using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace BEASTAdmin.UI.Pages.PaymentOption;

[Authorize(Roles = "SystemAdmin")]
public partial class PaymentOptionListModel : PageModel
{
    [BindProperty, DisplayName("PaymentType")]
    public string PaymentType { get; set; }
    public string PaymentOptionId { get; set; }
    public SelectList SelectList { get; set; }

    public List<PaymentOptionModel> paymentOptions { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<PaymentOptionListModel> _logger;
    private readonly PaymentOptionService _paymentOptionService;
    private readonly PaymentTypeService _paymentTypeService;

    public PaymentOptionListModel(ILogger<PaymentOptionListModel> logger, PaymentOptionService paymentOptionService, PaymentTypeService paymentTypeService)
    {
        this._logger = logger;
        this._paymentOptionService = paymentOptionService;
        this._paymentTypeService = paymentTypeService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name));

        var paginatedList = await _paymentOptionService.GetPaymentOptions(PageNumber);
        paymentOptions = paginatedList.Items;
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

        await _paymentOptionService.DeletePaymentOption(id, logModel);
        return RedirectToPage("/PaymentOption/PaymentOptionList", new { c = "popt", p = "poptl" });
    });

    public Task<IActionResult> OnPostFilter() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name));

        paymentOptions = await _paymentOptionService.GetDistinctPaymentOptions();
        TotalRecords = paymentOptions.Count;

        return Page();
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _paymentOptionService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}