using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.Country;

[Authorize(Roles = "SystemAdmin")]
public partial class CountryListModel : PageModel
{
    public List<CountryModel> Countries { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<CountryListModel> _logger;
    private readonly IConfiguration _config;
    private readonly CountryService _countryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public CountryListModel(ILogger<CountryListModel> logger, IConfiguration config, CountryService countryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._countryService = countryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _countryService.GetCountries(PageNumber);
        Countries = paginatedList.Items;
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

        var vehicle = await _countryService.GetCountryById(id);

		await _countryService.DeleteCountry(id, logModel);
        return RedirectToPage("/Country/CountryList", new { c = "country", p = "countryl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _countryService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}