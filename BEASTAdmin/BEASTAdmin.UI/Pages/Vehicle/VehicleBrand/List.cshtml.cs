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

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleBrand;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<VehicleBrandModel> vehicleBrands { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehicleBrandService _vehicleService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, VehicleBrandService vehicleService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._vehicleService = vehicleService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _vehicleService.GetVehicleBrands(PageNumber);
        vehicleBrands = paginatedList.Items;
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

        var vehicle = await _vehicleService.GetVehicleBrandById(id);
        
		await _vehicleService.DeleteVehicleBrand(id, logModel);
        return RedirectToPage("/Vehicle/VehicleBrand/List", new { c = "vehicleBrand", p = "vehicleBrandl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _vehicleService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}