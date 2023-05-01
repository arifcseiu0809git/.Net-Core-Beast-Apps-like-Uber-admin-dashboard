using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.City;

[Authorize(Roles = "SystemAdmin")]
public partial class CityListModel : PageModel
{
    public List<CityModel> Cities { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<CityListModel> _logger;
    private readonly IConfiguration _config;
    private readonly CityService _cityService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public CityListModel(ILogger<CityListModel> logger, IConfiguration config, CityService cityService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._cityService = cityService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _cityService.GetCities(PageNumber);
        Cities = paginatedList.Items;
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

        var vehicle = await _cityService.GetCityById(id);

		await _cityService.DeleteCity(id, logModel);
        return RedirectToPage("/City/CityList", new { c = "city", p = "cityl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _cityService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}