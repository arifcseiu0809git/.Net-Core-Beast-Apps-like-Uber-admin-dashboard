using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model.Common;
using MongoDB.Driver.Linq;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DriverStatus: PageModel
{
	[BindProperty]
	public DriverModel DriverModel { get; set; }

   public SelectList SelectList { get; set; }
	public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<DriverStatus> _logger;
    private readonly IConfiguration _config;
    private readonly DriverModelService _DriverModelService;
    private readonly SystemStatusService _SystemStatusService;
    
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public DriverStatus(ILogger<DriverStatus> logger, IConfiguration config, DriverModelService DriverModelService, SystemStatusService SystemStatusService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._DriverModelService = DriverModelService;
        this._SystemStatusService = SystemStatusService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
		SelectList = new SelectList(await _SystemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
		if (string.IsNullOrEmpty(id))
        {
			DriverModel = new DriverModel();
        }
        else
        {
			DriverModel = await _DriverModelService.GetDriverModelById(id);
        }
		//SelectList=(SelectList)SelectList.Where(x => x.Value == "Approved" || x.Value == "Pending"|| x.Value == "Blocked");
		return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
       
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

	        DriverModel.ModifiedBy = User.Identity.Name;
          await _DriverModelService.UpdateDriverStatus(DriverModel.Id, DriverModel, logModel);
          SuccessMessage = InformationMessages.Updated;

        return Page();
    });


}