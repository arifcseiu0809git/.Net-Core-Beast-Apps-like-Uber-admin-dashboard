using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Model.Common;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Service.Vehicle;

namespace BEASTAdmin.UI.Pages.PaymentMethod;

[Authorize(Roles = "SystemAdmin")]
public partial class PaymentMethodListModel : PageModel
{
    public List<PaymentMethodModel> PaymentMethods { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
	
	public SelectList PaymentTypes { get; set; }

	[BindProperty]
	public string PaymentOptionId { get; set; } = "";
	[BindProperty]
	public string PaymentTypeId { get; set; } = "";
	[BindProperty]
	public string AccountNo { get; set; } = "";

	[BindProperty]
	public string ContactNo { get; set; } = "";
	private readonly ILogger<PaymentMethodListModel> _logger;
    private readonly PaymentMethodService _paymentMethodService;
    private readonly PaymentTypeService _paymentTypeService;
    private readonly PaymentOptionService _paymentOptionService;


    public PaymentMethodListModel(ILogger<PaymentMethodListModel> logger, PaymentMethodService paymentMethodService, PaymentTypeService paymentTypeService, PaymentOptionService paymentOptionService)
    {
        this._logger = logger;
        this._paymentMethodService = paymentMethodService;
        this._paymentTypeService = paymentTypeService;
        this._paymentOptionService = paymentOptionService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _paymentMethodService.GetPaymentMethods(PageNumber);
		PaymentMethods = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
		await PopulatePageElements();
		return Page();
    });

	private async Task PopulatePageElements()
	{
		PaymentTypes = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name));
	}

	public Task<IActionResult> OnPostDelete(string id) =>
    TryCatch(async () =>
    {
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        await _paymentMethodService.DeletePaymentMethod(id, logModel);
        return RedirectToPage("/PaymentMethod/PaymentMethodList", new { c = "pmet", p = "pmetl" });
    });

	public Task<IActionResult> OnGetChangePaymentType(string paymentTypeId) =>
     TryCatch(async () =>
     {
	     var paymentOptions = await _paymentOptionService.GetPaymentOptionByPaymentTypeId(paymentTypeId);
	     return new JsonResult(paymentOptions);
     });

	public Task<IActionResult> OnPostFilter() =>
    TryCatch(async () =>
    {
        PaymentTypeId = PaymentTypeId == null ? "PaymentTypeId" : PaymentTypeId;
        PaymentOptionId = PaymentOptionId == null ? "PaymentOptionId" : PaymentOptionId;
        ContactNo = ContactNo == null ? "ContactNo" : ContactNo;
        AccountNo = AccountNo == null ? "AccountNo" : AccountNo;
        PaymentMethods = await _paymentMethodService.Filter(PaymentTypeId, PaymentOptionId, ContactNo, AccountNo);
        TotalRecords = PaymentMethods.Count;
		await PopulatePageElements();

		return Page();
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {

		PaymentTypeId = PaymentTypeId == null ? "PaymentTypeId" : PaymentTypeId;
		PaymentOptionId = PaymentOptionId == null ? "PaymentOptionId" : PaymentOptionId;
		ContactNo = ContactNo == null ? "ContactNo" : ContactNo;
		AccountNo = AccountNo == null ? "AccountNo" : AccountNo;
		var exportFile = await _paymentMethodService.Export(PaymentTypeId, PaymentOptionId, ContactNo, AccountNo);
		return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}