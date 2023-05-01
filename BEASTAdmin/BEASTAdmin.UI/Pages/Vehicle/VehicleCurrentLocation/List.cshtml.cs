using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using BEASTAdmin.Service.Vehicle;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Service;

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleCurrentLocation;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    [BindProperty]
	public int VehicleId { get; set; }
	public SelectList SelectList { get; set; }
	public List<VehicleCurrentLocationModel> vehicleCurrentLocations { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehicleCurrentLocationService _vehicleCurrentLocationService;
	private readonly VehicleBrandService _vehicleBrandService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, VehicleCurrentLocationService vehicleCurrentLocationService, VehicleBrandService vehicleBrandService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._vehicleCurrentLocationService = vehicleCurrentLocationService;
        this._vehicleBrandService = vehicleBrandService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
		SelectList = new SelectList(await _vehicleBrandService.GetDistinctVehicleBrand(), nameof(VehicleBrandModel.Id), nameof(VehicleBrandModel.Name));
		var paginatedList = await _vehicleCurrentLocationService.GetVehicleCurrentLocations(PageNumber);
		vehicleCurrentLocations = paginatedList.Items;
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

        var vehicleCurrentLocation = await _vehicleCurrentLocationService.GetVehicleCurrentLocationById(id);
        
		await _vehicleCurrentLocationService.DeleteVehicleCurrentLocation(id, logModel);
        return RedirectToPage("/Vehicle/VehicleCurrentLocation/List", new { c = "vehicleCurrentLocation", p = "vehicleCurrentLocationl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _vehicleCurrentLocationService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}