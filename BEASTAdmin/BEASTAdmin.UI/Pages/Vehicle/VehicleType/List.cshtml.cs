using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Service.Vehicle;

namespace BEASTAdmin.UI.Pages.VehicleType;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<VehicleTypeModel> Vehicles { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehicleTypeService _vehicleService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, VehicleTypeService vehicleService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
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
        var paginatedList = await _vehicleService.GetVehicleTypes(PageNumber);
        Vehicles = paginatedList.Items;
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

        var vehicle = await _vehicleService.GetVehicleTypeById(id);
        if (!string.IsNullOrEmpty(vehicle.ImageUrl))
			System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, "images\\VehicleType", vehicle.ImageUrl));

		await _vehicleService.DeleteVehicleType(id, logModel);
        return RedirectToPage("/Vehicle/VehicleType/List", new { c = "vehicleType", p = "vehicleTypel" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _vehicleService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}