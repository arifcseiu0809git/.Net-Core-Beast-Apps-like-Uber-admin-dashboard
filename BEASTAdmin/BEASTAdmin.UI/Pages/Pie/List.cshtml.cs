using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.Pie;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    [BindProperty, DisplayName("Category")]
    public string CategoryId { get; set; }
    public SelectList SelectList { get; set; }

    public List<PieModel> Pies { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly PieService _pieService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, PieService pieService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._pieService = pieService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));

        var paginatedList = await _pieService.GetPies(PageNumber);
        Pies = paginatedList.Items;
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

        var pie = await _pieService.GetPieById(id);
        if (!string.IsNullOrEmpty(pie.ImageUrl))
			System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, "images\\pie", pie.ImageUrl));

		await _pieService.DeletePie(id, logModel);
        return RedirectToPage("/Pie/List", new { c = "pie", p = "piel" });
    });

    public Task<IActionResult> OnPostFilter() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));

        Pies = await _pieService.GetPieByCategoryId(CategoryId);
        TotalRecords = Pies.Count;

        return Page();
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _pieService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}