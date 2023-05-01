using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service.Vehicle;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model.Common;

namespace BEASTAdmin.UI.Pages.Vehicle.Vehicles;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public VehiclesList Vehicle { get; set; }
	public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public SelectList VehicleBrandSelectList { get; set; }
	public SelectList SystemStatusSelectList { get; set; }
    public SelectList VehicleModelSelectList { get; set; }
    public SelectList VehicleTypeSelectList { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehiclesService _vehiclesService;
    private readonly VehicleBrandService _vehicleBrandService;
    private readonly VehicleModelService _vehicleModelService;
    private readonly SystemStatusService _systemStatusService;
    private readonly VehicleTypeService _vehicleTypeService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, VehiclesService vehiclesService, VehicleBrandService vehicleBrandService, VehicleModelService vehicleModelService, SystemStatusService systemStatusService, VehicleTypeService vehicleTypeService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._vehiclesService = vehiclesService;
        this._vehicleBrandService = vehicleBrandService;
        this._vehicleModelService = vehicleModelService;
        this._systemStatusService = systemStatusService;
        this._vehicleTypeService = vehicleTypeService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            Vehicle = new VehiclesList();
        }
        else
        {
			Vehicle = await _vehiclesService.GetVehicleById(id);
        }

        await PopulatePageElements();
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false)
        {
            await PopulatePageElements();
            return Page();
        }

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

     	if (string.IsNullOrEmpty(Vehicle.Id))
		{
			Vehicle.CreatedBy = User.Identity.Name;
			Vehicle.ApprovedBy = User.Identity.Name;
			await _vehiclesService.InsertVehicle(Vehicle, logModel);
			SuccessMessage = InformationMessages.Saved;
		}
		else
		{
			Vehicle.ModifiedBy = User.Identity.Name;
			Vehicle.ApprovedBy = User.Identity.Name;
			await _vehiclesService.UpdateVehicle(Vehicle.Id, Vehicle, logModel);
			SuccessMessage = InformationMessages.Updated;
		}

		await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
    private async Task PopulatePageElements()
    {
        VehicleBrandSelectList = new SelectList(await _vehicleBrandService.GetDistinctVehicleBrand(), nameof(VehicleBrandModel.Id), nameof(VehicleBrandModel.Name));
        SystemStatusSelectList = new SelectList(await _systemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
        VehicleTypeSelectList = new SelectList(await _vehicleTypeService.GetDistinctVehicleType(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
        VehicleModelSelectList = new SelectList(await _vehicleModelService.GetDistinctVehicleModel(), nameof(Core.Model.Vehicle.VehicleModel.Id), nameof(Core.Model.Vehicle.VehicleModel.Name));
    }

   
    #endregion
}