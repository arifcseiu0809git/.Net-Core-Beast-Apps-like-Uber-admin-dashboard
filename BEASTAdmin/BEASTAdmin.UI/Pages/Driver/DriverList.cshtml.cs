using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model.Common;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DriverList: PageModel
{
    public List<DriverModel> DriverModels { get; set; }
    [BindProperty]
    public string NID { get; set; } = "";
    [BindProperty]
    public string StatusId { get; set; } = "";
	[BindProperty]
	public string DrivingLicenseNo { get; set; } = "";
	public SelectList SelectList { get; set; }

	// Pagination
	[BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
	[BindProperty]
	public bool IsApproved { get; set; }

	private readonly ILogger<DriverList> _logger;
    private readonly IConfiguration _config;
    private readonly DriverModelService _DriverService;
    private readonly SystemStatusService _SystemStatusService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public DriverList(ILogger<DriverList> logger, IConfiguration config, DriverModelService DriverService, SystemStatusService SystemStatusService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._DriverService = DriverService;
        this._SystemStatusService = SystemStatusService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(bool approved) =>
    TryCatch(async () =>
    {
		SelectList = new SelectList(await _SystemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
		var paginatedList = await _DriverService.GetDriverModels(PageNumber, approved);
        IsApproved = approved;
		DriverModels = paginatedList.Items;
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

        var Driver = await _DriverService.GetDriverModelById(id);
		await _DriverService.DeleteDriverModel(id, logModel);
        return RedirectToPage("/Driver/DriverList", new { c = "Driver", p = "unApprovedDriver", approved = false });
    });

	public Task<IActionResult> OnPostFilter() =>
TryCatch(async () =>
{
	SelectList = new SelectList(await _SystemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
    StatusId = StatusId == null ? "StatusId" : StatusId;
	NID = NID == null ? "NID" : NID;
	DrivingLicenseNo = DrivingLicenseNo == null ? "DrivingLicenseNo" : DrivingLicenseNo;
	DriverModels = await _DriverService.Filter(IsApproved, StatusId, NID, DrivingLicenseNo);
	TotalRecords = DriverModels.Count;

	return Page();
});

	public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
		StatusId = StatusId == null ? "StatusId" : StatusId;
		NID = NID == null ? "NID" : NID;
		DrivingLicenseNo = DrivingLicenseNo == null ? "DrivingLicenseNo" : DrivingLicenseNo;
		var exportFile = await _DriverService.Export(IsApproved, StatusId, NID, DrivingLicenseNo);
		return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}