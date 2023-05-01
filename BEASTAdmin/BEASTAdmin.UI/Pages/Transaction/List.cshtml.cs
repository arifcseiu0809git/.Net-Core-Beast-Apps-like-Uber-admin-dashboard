using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.Transaction;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<TransactionModel> Transactions { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, TransactionService transactionService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._transactionService = transactionService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _transactionService.GetTransactions(PageNumber);
        Transactions = paginatedList.Items;
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

        var vehicle = await _transactionService.GetTransactionById(id);
   //     if (!string.IsNullOrEmpty(vehicle.ImageUrl))
			//System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, "images\\Transaction", vehicle.ImageUrl));

		await _transactionService.DeleteTransaction(id, logModel);
        return RedirectToPage("/Transaction/List", new { c = "transaction", p = "transactionl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _transactionService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}