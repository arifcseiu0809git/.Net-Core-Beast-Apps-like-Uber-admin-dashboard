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

namespace BEASTAdmin.UI.Pages.Trip;

[Authorize(Roles = "SystemAdmin")]
public partial class TripListModel : PageModel
{
    public List<TripModel> Trips { get; set; }

	[BindProperty]
	public string VehicleTypId { get; set; } = "";
	[BindProperty]
	public string StatusId { get; set; } = "";
	[BindProperty]
	public string DriverName { get; set; } = "";
	[BindProperty]
	public string PassengerName { get; set; } = "";	
	[BindProperty]
	public string ContactNo { get; set; } = "";
	public SelectList StatusList { get; set; }
	public SelectList VehicleTypeList { get; set; }

	[BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<TripListModel> _logger;
    private readonly IConfiguration _config;
    private readonly TripService _TripService;
	private readonly IWebHostEnvironment _hostEnvironment;
	private readonly SystemStatusService _SystemStatusService;
	private readonly VehicleTypeService _VehicleTypeService;
	public TripListModel(ILogger<TripListModel> logger, SystemStatusService oSystemStatusService, VehicleTypeService  oVehicleTypeService, IConfiguration config, TripService TripService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._TripService = TripService;
		this._hostEnvironment = hostEnvironment;
		this._SystemStatusService = oSystemStatusService;
		this._VehicleTypeService  = oVehicleTypeService;
	}

    public Task<IActionResult> OnGet(string statusId) =>
    TryCatch(async () =>
    {
		statusId = statusId == null ? "0" : statusId;
		//StatusList = new SelectList(await _SystemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
		VehicleTypeList = new SelectList(await _VehicleTypeService.GetDistinctVehicleType(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
		var paginatedList = await _TripService.GetTrips(statusId, PageNumber);
		StatusId = statusId;
		Trips = paginatedList.Items;
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

        var Trip = await _TripService.GetTripById(id);

		await _TripService.DeleteTrip(id, logModel);
        return RedirectToPage("/Trip/TripList", new { c = "Trip", p = "Tripl" });
    });
	public Task<IActionResult> OnPostFilter() =>
TryCatch(async () =>
{

VehicleTypeList = new SelectList(await _VehicleTypeService.GetDistinctVehicleType() , nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
    StatusId = StatusId == null ? "StatusId" : StatusId;
VehicleTypId = VehicleTypId == null ? "VehicleTypeId" : VehicleTypId;
	DriverName = DriverName == null ? "DriverName" : DriverName;
	PassengerName = PassengerName == null ? "PassengerName" : PassengerName;
	ContactNo = ContactNo == null ? "ContactNo" : ContactNo;
	Trips = await _TripService.Filter(StatusId, VehicleTypId, DriverName, PassengerName, ContactNo);
    TotalRecords = Trips.Count;

return Page();
});

	public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
		StatusId = StatusId == null ? "StatusId" : StatusId;
		VehicleTypId = VehicleTypId == null ? "VehicleTypeId" : VehicleTypId;
		DriverName = DriverName == null ? "DriverName" : DriverName;
		PassengerName = PassengerName == null ? "PassengerName" : PassengerName;
		ContactNo = ContactNo == null ? "ContactNo" : ContactNo;
		var exportFile = await _TripService.Export(StatusId, VehicleTypId, DriverName, PassengerName, ContactNo);
		return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}