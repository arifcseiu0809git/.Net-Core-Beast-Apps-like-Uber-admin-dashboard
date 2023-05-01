using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model.Common;
using System.Security.Cryptography;
using BEASTAdmin.Service.Vehicle;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.UI.Pages.ManualRideBooking;

[Authorize(Roles = "SystemAdmin")]
public partial class ManualRideBookingListModel : PageModel
{
    public List<TripInitialModel> TripInitials { get; set; }

	[BindProperty]
	public string VehicleTypId { get; set; } = "";
	[BindProperty]
	public string StatusId { get; set; } = "";
	[BindProperty]
	public string DriverName { get; set; } = "";
	[BindProperty]
	public string PassengerName { get; set; } = "";
	public SelectList StatusList { get; set; }
	public SelectList VehicleTypeList { get; set; }

	[BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ManualRideBookingListModel> _logger;
    private readonly IConfiguration _config;
    private readonly TripInitialService _tripInitialService;
	private readonly IWebHostEnvironment _hostEnvironment;
	private readonly SystemStatusService _SystemStatusService;
	private readonly VehicleTypeService _VehicleTypeService;
	public ManualRideBookingListModel(ILogger<ManualRideBookingListModel> logger, SystemStatusService oSystemStatusService, VehicleTypeService  oVehicleTypeService, IConfiguration config, TripInitialService tripInitialService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._tripInitialService = tripInitialService;
		this._hostEnvironment = hostEnvironment;
		this._SystemStatusService = oSystemStatusService;
		this._VehicleTypeService  = oVehicleTypeService;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {

		StatusList = new SelectList(await _SystemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
		VehicleTypeList = new SelectList(await _VehicleTypeService.GetDistinctVehicleType(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
		var paginatedList = await _tripInitialService.GetTripInitials(PageNumber);
        TripInitials = paginatedList.Items;
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

        var tripInitial = await _tripInitialService.GetTripInitialById(id);

		await _tripInitialService.DeleteTripInitial(id, logModel);
        return RedirectToPage("/ManualRideBooking/ManualRideBookingList", new { c = "manualRideBooking", p = "manualRideBookingl" });
    });
	public Task<IActionResult> OnPostFilter() =>
TryCatch(async () =>
{
StatusList = new SelectList(await _SystemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
VehicleTypeList = new SelectList(await _VehicleTypeService.GetDistinctVehicleType() , nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
    StatusId = StatusId == null ? "StatusId" : StatusId;
VehicleTypId = VehicleTypId == null ? "VehicleTypeId" : VehicleTypId;
	DriverName = DriverName == null ? "DriverName" : DriverName;
	PassengerName = PassengerName == null ? "PassengerName" : PassengerName;
	TripInitials = await _tripInitialService.Filter(StatusId, VehicleTypId, DriverName, PassengerName);
    TotalRecords = TripInitials.Count;

return Page();
});

	public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
		StatusId = StatusId == null ? "StatusId" : StatusId;
		VehicleTypId = VehicleTypId == null ? "VehicleTypeId" : VehicleTypId;
		DriverName = DriverName == null ? "DriverName" : DriverName;
		PassengerName = PassengerName == null ? "PassengerName" : PassengerName;
		var exportFile = await _tripInitialService.Export(StatusId, VehicleTypId, DriverName, PassengerName);
		return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}