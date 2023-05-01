using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Service.Vehicle;

namespace BEASTAdmin.UI.Pages.VehicleModel;

[Authorize(Roles = "SystemAdmin")]
public partial class VehicleListModel : PageModel
{
    public List<Core.Model.Vehicle.VehicleModel> VehicleModels { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<VehicleListModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehicleModelService _vehicleService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public VehicleListModel(ILogger<VehicleListModel> logger, IConfiguration config, VehicleModelService vehicleService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._vehicleService = vehicleService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _vehicleService.GetVehicleModels(PageNumber);
		VehicleModels = paginatedList.Items;
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

        var vehicle = await _vehicleService.GetVehicleModelById(id);
		await _vehicleService.DeleteVehicleModel(id, logModel);
        return RedirectToPage("/Vehicle/VehicleModel/VehicleListModel", new { c = "vehicleModel", p = "vehicleModell" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _vehicleService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}