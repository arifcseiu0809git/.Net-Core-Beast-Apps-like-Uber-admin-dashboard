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
using BEASTAdmin.Service.Vehicle;
using BEASTAdmin.Core.Model.Vehicle;
using Microsoft.AspNetCore.Identity;
using BEASTAdmin.UI.Areas.Identity.Data;

namespace BEASTAdmin.UI.Pages.VehicleModel;

[Authorize(Roles = "SystemAdmin")]
public partial class VehicleModelUpsert : PageModel
{
    [BindProperty]
    public Core.Model.Vehicle.VehicleModel VehicleModel { get; set; }
	public SelectList SelectList { get; set; }
	public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<VehicleModelUpsert> _logger;
    private readonly IConfiguration _config;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly VehicleModelService _vehicleModelService;
    private readonly VehicleBrandService _vehicleBrandService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public VehicleModelUpsert(UserManager<ApplicationUser> userManager, ILogger<VehicleModelUpsert> logger, IConfiguration config, VehicleModelService vehicleModelService, VehicleBrandService vehicleBrandService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._vehicleModelService = vehicleModelService;
        this._vehicleBrandService = vehicleBrandService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
		SelectList = new SelectList(await _vehicleBrandService.GetDistinctVehicleBrands(), nameof(VehicleBrandModel.Id), nameof(VehicleBrandModel.Name));
		if (string.IsNullOrEmpty(id))
        {
			VehicleModel = new BEASTAdmin.Core.Model.Vehicle.VehicleModel();
        }
        else
        {
			VehicleModel = await _vehicleModelService.GetVehicleModelById(id);
        }
        
		//  await PopulatePageElements();
		return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false)
        {
            //await PopulatePageElements();
            return Page();
        }



        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        if (string.IsNullOrEmpty(VehicleModel.Id))
        {
			VehicleModel.CreatedBy = User.Identity.Name;
			await _vehicleModelService.InsertVehicleMOdel(VehicleModel, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
			VehicleModel.ModifiedBy = User.Identity.Name;
			await _vehicleModelService.UpdateVehicleModel(VehicleModel.Id, VehicleModel, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

       // await PopulatePageElements();
        return Page();
    });


}