using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service.Vehicle;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.Vehicle.Vehicles;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    [BindProperty, DisplayName("Brand")]
    public string vehicleBrandId { get; set; }
	//[BindProperty, DisplayName("Type")]
	//public int vehicleTypeId { get; set; }
	[BindProperty, DisplayName("Model")]
	public string vehicleModelId { get; set; }
    public SelectList SelectListBrand { get; set; }
    //public SelectList SelectListType { get; set; }
    public SelectList SelectListModel { get; set; }
    public SelectList SelectListStatus { get; set; }
    public List<VehiclesList> vehicles { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehiclesService _vehiclesService;
    private readonly VehicleBrandService _vehicleBrandService;
    private readonly SystemStatusService _systemStatusService;
    //private readonly VehicleTypeService _vehicleTypeService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, VehiclesService vehiclesService, VehicleBrandService vehicleBrandService, SystemStatusService systemStatusService,/*VehicleTypeService vehicleTypeService,*/ IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._vehiclesService = vehiclesService;
        this._vehicleBrandService = vehicleBrandService;
        this._systemStatusService = systemStatusService;
        //this._vehicleTypeService = vehicleTypeService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        SelectListBrand = new SelectList(await _vehicleBrandService.GetDistinctVehicleBrand(), nameof(VehicleBrandModel.Id), nameof(VehicleBrandModel.Name));
        SelectListStatus = new SelectList(await _systemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
        //SelectListModel = new SelectList(await _vehicleBrandService.GetDistinctVehicleBrand(), nameof(VehicleModel.Id), nameof(VehicleBrand.Name));
        //SelectListType = new SelectList(await _vehicleTypeService.GetDistinctVehicleType(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));

        var paginatedList = await _vehiclesService.GetVehicles(PageNumber);
        vehicles = paginatedList.Items;
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

        var pie = await _vehiclesService.GetVehicleById(id);
        
		await _vehiclesService.DeleteVehicle(id, logModel);
        return RedirectToPage("/Vehicle/Vehicles/List", new { c = "vehicles", p = "vehiclesList" });
    });

    //public Task<IActionResult> OnPostFilter() =>
    //TryCatch(async () =>
    //{
    //    SelectListType = new SelectList(await _vehicleTypeService.GetDistinctVehicleType(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));

    //    vehicles = await _vehiclesService.GetVehicleByTypeId(vehicleTypeId);
    //    TotalRecords = vehicles.Count;

    //    return Page();
    //});

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _vehiclesService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}