using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using BEASTAdmin.Service;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace BEASTAdmin.UI.Pages.Passenger;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public PassengerModel Passenger { get; set; }

    [BindProperty, DisplayName("Passenger Image")]
    public IFormFile? PassengerImage { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }
    public SelectList SystemStatusSelectList { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly PassengerService _passengerService;
    private readonly SystemStatusService _systemStatusService;
    private readonly IWebHostEnvironment _hostEnvironment;

    public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, PassengerService passengerService, SystemStatusService systemStatusService, IWebHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _config = config;
        _passengerService = passengerService;
        _systemStatusService = systemStatusService;
        _hostEnvironment = hostEnvironment;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            Passenger = new PassengerModel();
        }
        else
        {
            Passenger = await _passengerService.GetPassengerById(id);
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

        if (string.IsNullOrEmpty(Passenger.Id))
        {
            Passenger.CreatedBy = User.Identity.Name;
            await _passengerService.InsertPassenger(Passenger, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            Passenger.ModifiedBy = User.Identity.Name;
            await _passengerService.UpdatePassenger(Passenger.Id, Passenger, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"

    private async Task PopulatePageElements()
    {
        SystemStatusSelectList = new SelectList(await _systemStatusService.GetDistinctSystemStatus(), nameof(SystemStatusModel.Id), nameof(SystemStatusModel.Name));
        //ImageURL = "/images/" + (string.IsNullOrEmpty(Pie.ImageUrl) ? "NoImage.jpg" : "pie/" + Pie.ImageUrl);
    }

    //  private async Task<string> UploadFile()
    //  {
    //      string uploadFolder = "";
    //      string uniqueFileName = "";
    //      string filePath = "";

    //uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\Passenger");

    //// Delete existing image
    //if (!string.IsNullOrEmpty(VehicleType.ImageUrl))
    //          System.IO.File.Delete(Path.Combine(uploadFolder, VehicleType.ImageUrl));

    //      // Upload new image
    //      uniqueFileName = Guid.NewGuid().ToString() + "-" + VehicleTypeImage.FileName;
    //      filePath = Path.Combine(uploadFolder, uniqueFileName);
    //      using (var fileStream = new FileStream(filePath, FileMode.Create))
    //      {
    //          await VehicleTypeImage.CopyToAsync(fileStream);
    //      }

    //      return uniqueFileName;
    //  }
    //   private async Task PopulatePageElements()
    //   {
    //     ImageURL = "/images/" + (string.IsNullOrEmpty(VehicleType.ImageUrl) ? "NoImage.jpg" : "VehicleType/" + VehicleType.ImageUrl);
    //}

    //  private async Task<string> UploadFile()
    //  {
    //      string uploadFolder = "";
    //      string uniqueFileName = "";
    //      string filePath = "";

    //uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\Passenger");

    //// Delete existing image
    //if (!string.IsNullOrEmpty(VehicleType.ImageUrl))
    //          System.IO.File.Delete(Path.Combine(uploadFolder, VehicleType.ImageUrl));

    //      // Upload new image
    //      uniqueFileName = Guid.NewGuid().ToString() + "-" + VehicleTypeImage.FileName;
    //      filePath = Path.Combine(uploadFolder, uniqueFileName);
    //      using (var fileStream = new FileStream(filePath, FileMode.Create))
    //      {
    //          await VehicleTypeImage.CopyToAsync(fileStream);
    //      }

    //      return uniqueFileName;
    //  }
    #endregion
}