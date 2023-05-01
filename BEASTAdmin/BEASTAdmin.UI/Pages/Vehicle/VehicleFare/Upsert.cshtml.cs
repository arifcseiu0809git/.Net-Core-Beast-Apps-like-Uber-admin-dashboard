using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Data;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleFare
{
    [Authorize(Roles = "SystemAdmin")]

    public partial class UpsertModel : PageModel
    {
        [BindProperty]
        public VehicleFareModel vehicleFare { get; set; }

        [BindProperty, DisplayName("Vehicle Fare")]
        public string Name { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public SelectList VehicleTypeList { get; set; }

        private readonly ILogger<UpsertModel> _logger;
        private readonly IConfiguration _config;
        private readonly VehicleFareService _vehicleFareService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly VehicleTypeService _vehicleTypeService;

		public UpsertModel(
            ILogger<UpsertModel> logger,
            IConfiguration config,
            VehicleFareService VehicleFareService,
            VehicleTypeService vehicleTypeService,
            IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _config = config;
            _vehicleFareService = VehicleFareService;
            _hostEnvironment = hostEnvironment;
            _vehicleTypeService = vehicleTypeService;
        }

        public Task<IActionResult> OnGet(string id) =>
		TryCatch(async () =>
		{
			if (string.IsNullOrEmpty(id))
			{
				vehicleFare = new VehicleFareModel();
			}
			else
			{
				vehicleFare = await _vehicleFareService.GetVehicleFareById(id);
			}
			await PopulatePageElements();
			return Page();
		});

		private async Task PopulatePageElements()
		{
			VehicleTypeList = new SelectList(await _vehicleTypeService.GetDistinctVehicleTypes(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
		}

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

            if (string.IsNullOrEmpty(vehicleFare.Id))
            {
                vehicleFare.CreatedBy = User.Identity.Name;
                await _vehicleFareService.InsertVehicleFare(vehicleFare, logModel);
                SuccessMessage = InformationMessages.Saved;
            }
            else
            {
                vehicleFare.ModifiedBy = User.Identity.Name;
                await _vehicleFareService.UpdateVehicleFare(vehicleFare.Id, vehicleFare, logModel);
                SuccessMessage = InformationMessages.Updated;
            }
			await PopulatePageElements();
			return Page();
        });
    }
}
