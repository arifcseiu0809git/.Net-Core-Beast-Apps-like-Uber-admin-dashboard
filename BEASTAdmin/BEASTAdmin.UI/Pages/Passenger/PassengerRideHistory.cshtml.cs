using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model.Passenger;
using BEASTAdmin.Core.Model.Common;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.UI.Pages.Passenger;

[Authorize(Roles = "SystemAdmin")]
public partial class PassengerRideHistory : PageModel
{
    public List<PassengerRideHistoryModel> RideHistories { get; set; }
    [BindProperty]
    public string PassengerId { get; set; }
    [BindProperty]
    public string PassengerName { get; set; }
	[BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public SelectList SelectList { get; set; }

    private readonly ILogger<PassengerRideHistory> _logger;
    private readonly IConfiguration _config;
    private readonly PassengerService _passengerService;
    private readonly SystemStatusService _systemStatusService;
    private readonly IWebHostEnvironment _hostEnvironment;

    public PassengerRideHistory(ILogger<PassengerRideHistory> logger, IConfiguration config, PassengerService passengerService, SystemStatusService systemStatusService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._passengerService = passengerService;
        this._systemStatusService = systemStatusService;
        this._hostEnvironment = hostEnvironment;
    }

	public Task<IActionResult> OnGet(string id, string name) =>
	TryCatch(async () =>
	{
		var paginatedList = await _passengerService.GetPassengerRideHistoriesById(id, PageNumber);
		PassengerId = id;
        PassengerName = name;
		RideHistories = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;

        return Page();
	});
	public Task<IActionResult> OnPostExport(string passengerId) =>
	TryCatch(async () =>
	{
		var exportFile = await _passengerService.ExportPassengerRideHistory(passengerId);
		return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
	});
}