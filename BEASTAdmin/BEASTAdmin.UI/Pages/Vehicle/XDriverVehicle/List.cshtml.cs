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

namespace BEASTAdmin.UI.Pages.Vehicle.XDriverVehicle;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<XDriverVehicleModel> xDriverVehicleList { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly IConfiguration _config;
    private readonly XDriverVehicleService _xDriverVehicleService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public ListModel(ILogger<ListModel> logger, IConfiguration config, XDriverVehicleService xDriverVehicleService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._xDriverVehicleService = xDriverVehicleService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _xDriverVehicleService.GetDriverVehicles(PageNumber);
		xDriverVehicleList = paginatedList.Items;
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

        //var vehicle = await _xDriverVehicleService.GetDriverVehicleById(id);
        
		await _xDriverVehicleService.DeletexDriverVehicle(id, logModel);
        return RedirectToPage("/Vehicle/XDriverVehicle/List", new { c = "xDriverVehicle", p = "xDriverVehiclel" });
    });
    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _xDriverVehicleService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}