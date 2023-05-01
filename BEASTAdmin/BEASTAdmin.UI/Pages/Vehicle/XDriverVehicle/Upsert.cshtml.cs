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

namespace BEASTAdmin.UI.Pages.Vehicle.XDriverVehicle;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public XDriverVehicleUpsertModel xDriverVehicle { get; set; }

    //[BindProperty, DisplayName("Vehicle Brand Name")]
    //public string Name { get; set; }
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }


    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly XDriverVehicleService _xDriverVehicleService;
    private readonly IWebHostEnvironment _hostEnvironment;

    public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, XDriverVehicleService xDriverVehicleService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._xDriverVehicleService = xDriverVehicleService;
        this._hostEnvironment = hostEnvironment;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            xDriverVehicle = new XDriverVehicleUpsertModel();
        }
        else
        {
            xDriverVehicle = await _xDriverVehicleService.GetDriverVehicleById(id);
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

        if (string.IsNullOrEmpty(xDriverVehicle.Id))
        {
            xDriverVehicle.CreatedBy = User.Identity.Name;
            await _xDriverVehicleService.InsertDriverVehicle(xDriverVehicle, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            xDriverVehicle.ModifiedBy = User.Identity.Name;
            await _xDriverVehicleService.Update(xDriverVehicle.Id, xDriverVehicle, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        // await PopulatePageElements();
        return Page();
    });

	public Task<IActionResult> OnGetSearchDriver(string prefix) =>
	TryCatch(async () =>
	{
  //      var drivers = new List<XDriverVehicleUpsertModel> 
  //      {
  //          new XDriverVehicleUpsertModel { DriverNameWithLicenseNo = "Alve (asda)", UserId = new Guid().ToString()},
		//	new XDriverVehicleUpsertModel { DriverNameWithLicenseNo = "Atik (sadas)", UserId = new Guid().ToString()}
		//};
        var selectListItems = new SelectList(await _xDriverVehicleService.GetDriverBySearchPrefix(prefix), nameof(XDriverVehicleUpsertModel.UserId), nameof(XDriverVehicleUpsertModel.DriverNameWithLicenseNo));

		return new JsonResult(selectListItems);
	});

	public Task<IActionResult> OnGetSearchVehicle(string prefix) =>
	TryCatch(async () =>
	{
		var selectListItems = new SelectList(await _xDriverVehicleService.GetVehiclesBySearchPrefix(prefix), nameof(XDriverVehicleUpsertModel.VehicleId), nameof(XDriverVehicleUpsertModel.VehicleRegistrationNumberWithVehicleType));
		return new JsonResult(selectListItems);
	});
}