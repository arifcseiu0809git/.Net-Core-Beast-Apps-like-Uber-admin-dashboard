using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.TransactionResponse;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<TransactionResponseModel> TransactionResponses { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly TransactionResponseService _transactionResponseService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, TransactionResponseService transactionResponseService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._transactionResponseService = transactionResponseService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _transactionResponseService.GetTransactionResponses(PageNumber);
        TransactionResponses = paginatedList.Items;
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

        var vehicle = await _transactionResponseService.GetTransactionResponseById(id);
   //     if (!string.IsNullOrEmpty(vehicle.ImageUrl))
			//System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, "images\\VehicleType", vehicle.ImageUrl));

		await _transactionResponseService.DeleteTransactionResponse(id, logModel);
        return RedirectToPage("/TransactionResponse/List", new { c = "transactionResponse", p = "transactionResponsel" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _transactionResponseService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}