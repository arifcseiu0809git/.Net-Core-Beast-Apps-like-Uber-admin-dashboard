using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

namespace BEASTAdmin.UI.Pages.Passenger;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<PassengerModel> Passengers { get; set; }
	[BindProperty]
	public string StatusId { get; set; } = "";
	[BindProperty]
	public string City { get; set; } = "";
	[BindProperty]
	public string ContactNo { get; set; } = "";
	// Pagination
	[BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public SelectList SelectList { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly PassengerService _passengerService;
    private readonly SystemStatusService _systemStatusService;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ListModel(ILogger<ListModel> logger, IConfiguration config, PassengerService passengerService, SystemStatusService systemStatusService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._passengerService = passengerService;
        this._systemStatusService = systemStatusService;
        this._hostEnvironment = hostEnvironment;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _systemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
        var paginatedList = await _passengerService.GetPassengers(PageNumber);
        Passengers = paginatedList.Items;
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

        var passenger = await _passengerService.GetPassengerById(id);
        await _passengerService.DeletePassenger(id, logModel);
        return RedirectToPage("/Passenger/List", new { c = "passenger", p = "passengerl" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
		StatusId = StatusId == null ? "StatusId" : StatusId;
		City = City == null ? "City" : City;
		ContactNo = ContactNo == null ? "ContactNo" : ContactNo;
		var exportFile = await _passengerService.Export(StatusId, City, ContactNo);
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
	public Task<IActionResult> OnPostFilter() =>
    TryCatch(async () =>
    {
	    SelectList = new SelectList(await _systemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
	    StatusId = StatusId == null ? "StatusId" : StatusId;
        City = City == null ? "City" : City;
	    ContactNo = ContactNo == null ? "ContactNo" : ContactNo;
		Passengers = await _passengerService.Filter(StatusId, City, ContactNo);
	    TotalRecords = Passengers.Count;

	    return Page();
    });
}