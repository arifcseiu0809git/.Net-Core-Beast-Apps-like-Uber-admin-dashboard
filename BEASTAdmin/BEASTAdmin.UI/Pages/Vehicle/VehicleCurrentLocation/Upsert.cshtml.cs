using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service.Vehicle;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Service;

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleCurrentLocation;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
	[BindProperty]
	public VehicleCurrentLocationModel vehicleCurrentLocation { get; set; }
	public string LastUpdateAt { get; set; }
	public string ErrorMessage { get; set; }
	public string SuccessMessage { get; set; }
	public SelectList VehicleBrandSelectList { get; set; }
	private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehicleCurrentLocationService _vehicleCurrentLocationService;
    private readonly VehicleBrandService _vehicleBrandService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, VehicleCurrentLocationService vehicleCurrentLocationService, VehicleBrandService vehicleBrandService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._vehicleCurrentLocationService = vehicleCurrentLocationService;
        this._vehicleBrandService = vehicleBrandService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            vehicleCurrentLocation = new VehicleCurrentLocationModel();
        }
        else
        {
            vehicleCurrentLocation = await _vehicleCurrentLocationService.GetVehicleCurrentLocationById(id);
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

        if (string.IsNullOrEmpty(vehicleCurrentLocation.Id))
        {
            vehicleCurrentLocation.VehicleId = "d39c8ab7-4dd7-4220-b3c1-80fd1c2602e1";
            vehicleCurrentLocation.CreatedBy = User.Identity.Name;
            await _vehicleCurrentLocationService.InsertVehicleCurrentLocation(vehicleCurrentLocation, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            vehicleCurrentLocation.ModifiedBy = User.Identity.Name;
            await _vehicleCurrentLocationService.UpdateVehicleCurrentLocation(vehicleCurrentLocation.Id, vehicleCurrentLocation, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        await PopulatePageElements();
        return Page();
    });

	#region "Helper Methods"
	private async Task PopulatePageElements()
    {
		VehicleBrandSelectList = new SelectList(await _vehicleBrandService.GetDistinctVehicleBrand(), nameof(VehicleBrandModel.Id), nameof(VehicleBrandModel.Name));
		//VehicleModelSelectList = new SelectList(await _vehicleCurrentLocationService.GetDistinctVehicleModelIncludingBrand(vehicleCurrentLocation.Id), nameof(VehicleBrandModel.Id), nameof(VehicleBrandModel.Model));
	}
	#endregion
}