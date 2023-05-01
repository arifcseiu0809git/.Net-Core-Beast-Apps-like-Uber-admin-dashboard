using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.TransactionRequest;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<TransactionRequestModel> TransactionRequests { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly TransactionRequestService _transactionRequestService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, TransactionRequestService transactionRequestService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._transactionRequestService = transactionRequestService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _transactionRequestService.GetTransactionRequests(PageNumber);
        TransactionRequests = paginatedList.Items;
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

        var vehicle = await _transactionRequestService.GetTransactionRequestById(id);
   //     if (!string.IsNullOrEmpty(vehicle.ImageUrl))
			//System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, "images\\VehicleType", vehicle.ImageUrl));

		await _transactionRequestService.DeleteTransactionRequest(id, logModel);
        return RedirectToPage("/TransactionRequest/List", new { c = "transactionReques", p = "transactionRequesl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _transactionRequestService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}