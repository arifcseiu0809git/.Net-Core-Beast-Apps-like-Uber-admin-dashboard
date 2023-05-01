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

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleBrand;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public VehicleBrandModel vehicleBrand { get; set; }

    [BindProperty, DisplayName("Vehicle Brand Name")]
    public string Name { get; set; }
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
   

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly VehicleBrandService _VehicleBrandService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, VehicleBrandService VehicleBrandService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._VehicleBrandService = VehicleBrandService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            vehicleBrand = new VehicleBrandModel();
        }
        else
        {
            vehicleBrand = await _VehicleBrandService.GetVehicleBrandById(id);
        }

       // await PopulatePageElements();
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

        if (string.IsNullOrEmpty(vehicleBrand.Id))
        {
            vehicleBrand.CreatedBy = User.Identity.Name;
            await _VehicleBrandService.InsertVehicleBrand(vehicleBrand, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            vehicleBrand.ModifiedBy = User.Identity.Name;
            await _VehicleBrandService.UpdateVehicleBrand(vehicleBrand.Id, vehicleBrand, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

       // await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
 //   private async Task PopulatePageElements()
 //   {
 //     ImageURL = "/images/" + (string.IsNullOrEmpty(VehicleBrand.ImageUrl) ? "NoImage.jpg" : "VehicleBrand/" + VehicleBrand.ImageUrl);
	//}

    private async Task<string> UploadFile()
    {
        string uploadFolder = "";
        string uniqueFileName = "";
        string filePath = "";

		uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\VehicleBrand");

		// Delete existing image
		
        // Upload new image
        //uniqueFileName = Guid.NewGuid().ToString() + "-" + VehicleBrandImage.FileName;
        //filePath = Path.Combine(uploadFolder, uniqueFileName);
        //using (var fileStream = new FileStream(filePath, FileMode.Create))
        //{
        //    await VehicleBrandImage.CopyToAsync(fileStream);
        //}

        return uniqueFileName;
    }
    #endregion
}