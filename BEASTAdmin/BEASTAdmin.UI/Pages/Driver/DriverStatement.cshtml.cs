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

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DriverStatement: PageModel
{
    public List<TripModel> TripModels { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
	public int CancelTrips { get; set; }
    public string DriverName { get; set; }
	public int CompleteTrips { get; set; }
	public decimal Revenues { get; set; }
    public DriverModel DriverModel { get; set; }

	private readonly ILogger<DriverStatement> _logger;
    private readonly IConfiguration _config;
    private readonly DriverModelService _DriverModelService;
    private readonly TripService _TripService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public DriverStatement(ILogger<DriverStatement> logger, IConfiguration config, TripService TripService, DriverModelService DriverModelService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._TripService = TripService;
        this._DriverModelService = DriverModelService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string Id , string DriverName) =>
    TryCatch(async () =>
    {
		DriverModel = await _DriverModelService.GetDriverModelById(Id);
        var paginatedList = await _TripService.GetTripsByDriverId(Id, PageNumber);
        
		TripModels = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
        this.DriverName = DriverName;
		CancelTrips = TripModels.Where(x => x.StatusName== "Cancel").Count();
		CompleteTrips = TripModels.Where(x => x.StatusName == "Success").Count();
		Revenues = TripModels.Where(x => x.StatusName == "Success").Sum(x=>x.Total);
        
		return Page();
    });

  //  public Task<IActionResult> OnPostDelete(string id) =>
  //  TryCatch(async () =>
  //  {
  //      LogModel logModel = new LogModel();
  //      logModel.UserName = User.Identity.Name;
  //      logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
  //      logModel.IP = Utility.GetIPAddress(Request);

  //      var Driver = await _TripService.GetTripModelById(id);
		//await _TripService.DeleteTripModel(id, logModel);
  //      return RedirectToPage("/Driver/DriverStatement", new { c = "Driver", p = false });
  //  });

}