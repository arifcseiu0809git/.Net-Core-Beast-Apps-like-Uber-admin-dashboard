using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Service.Vehicle;

using BEASTAdmin.Core.Model.Common;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using BEASTAdmin.UI.Areas.Identity.Data;

namespace BEASTAdmin.UI.Pages.ManualRideBooking;

[Authorize(Roles = "SystemAdmin")]
public partial class ManualRideBookingUpsertModel : PageModel
{
    [BindProperty]
    public TripInitialModel TripInitial { get; set; }

    [BindProperty, DisplayName("TripInitial Image")]
    public IFormFile? TripInitialImage { get; set; }
    [BindProperty, DisplayName("Pick Up Date Time")]
    public string RequestTime { get; set; }
    [BindProperty]
    public string DriverName { get; set; } = ""; //for filtering
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }
    public SelectList CountrySelectList { get; set; }
    public SelectList CitySelectList { get; set; }
    public SelectList VehicleTypeSelectList { get; set; }
    public SelectList PassengerSelectList { get; set; }
    [BindProperty]
    public  List<DriverModel> DriverList { get; set; }
    

    private readonly ILogger<ManualRideBookingUpsertModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly TripInitialService _tripInitialService;
    private readonly CityService _cityService;
    private readonly CountryService _countryService;
    private readonly VehicleTypeService _vehicleTypeService;
    private readonly PassengerService _passengerService;
    private readonly SavedAddressService _savedAddressService;
    private readonly DriverModelService _driverModelService;
    
    private readonly IWebHostEnvironment _hostEnvironment;

	public ManualRideBookingUpsertModel(UserManager<ApplicationUser> userManager, ILogger<ManualRideBookingUpsertModel> logger, IConfiguration config, TripInitialService tripInitialService, CityService cityService, CountryService countryService, VehicleTypeService vehicleTypeService, PassengerService passengerService, SavedAddressService savedAddressService, DriverModelService driverModelService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._tripInitialService = tripInitialService;
        this._cityService = cityService;
        this._countryService = countryService;
        this._vehicleTypeService = vehicleTypeService;
        this._passengerService = passengerService;
        this._driverModelService = driverModelService;
        this._hostEnvironment = hostEnvironment;
        this._savedAddressService  = savedAddressService;
        this._userManager = userManager;

    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            TripInitial = new TripInitialModel();
        }
        else
        {
            TripInitial = await _tripInitialService.GetTripInitialById(id);
        }

        await PopulatePageElements();
        return Page();
    });
    [ValidateAntiForgeryToken]
    public Task<IActionResult> OnPostSave(TripInitialModel oTrip) =>
    TryCatch(async () =>
    {
        TripInitial = oTrip;//set
        if (await ValidatePost() == false)
        {
            TripInitial.Message = "Invalid entry";
            return new  JsonResult(TripInitial);
        }

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        if (string.IsNullOrEmpty(TripInitial.Id))
        {
            TripInitial.CreatedBy = _userManager.GetUserId(User);
            await _tripInitialService.InsertTripInitial(TripInitial, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            TripInitial.ModifiedBy = _userManager.GetUserId(User);
            await _tripInitialService.UpdateTripInitial(TripInitial.Id, TripInitial, logModel);
            SuccessMessage = InformationMessages.Updated;
        }
       // return new JsonResult(TripInitial);
      //  await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
    private async Task PopulatePageElements()
    {
        CountrySelectList = new SelectList(await _countryService.GetDistinctCountries(), nameof(CountryModel.Id), nameof(CountryModel.CountryName));
        //CitySelectList = new SelectList(await _cityService.GetDistinctCities(), nameof(CityModel.Id), nameof(CityModel.Name));
        VehicleTypeSelectList = new SelectList(await _vehicleTypeService.GetDistinctVehicleTypes(), nameof(VehicleTypeModel.Id), nameof(VehicleTypeModel.Name));
        PassengerSelectList = new SelectList(await _passengerService.GetDistinctPassengers(), nameof(PassengerModel.Id), nameof(PassengerModel.MobileNumber));
        DriverList = await _driverModelService.GetActiveDrivers();
    }



    public Task<IActionResult> OnGetFilterDriver(string Driver, string vehicleTypeId) =>
          TryCatch(async () =>
          {
              Driver= Driver == null ? "" : Driver;
            var   DriverList = (await _driverModelService.GetActiveDrivers()).Where(x =>(x.FirstName + x.MiddleName + x.LastName).ToLower().Contains(Driver.ToLower())).ToList();
              if (!string.IsNullOrEmpty(vehicleTypeId))
              {
                  DriverList = DriverList.Where(x => x.VehicleTypeId == vehicleTypeId).ToList();
              }
              return new JsonResult(DriverList);
          });

    public Task<IActionResult> OnGetDriverById(string id) =>
      TryCatch(async () =>
      {
          var oDriver = (await _driverModelService.GetDriverModelById(id));
          return new JsonResult(oDriver);
      });  
    public Task<IActionResult> OnGetCityByCountryId(string id) =>
      TryCatch(async () =>
      {
          var oCities = (await _cityService.GetCityByCuntryId(id));
          return new JsonResult(oCities);
      });
    public Task<IActionResult> OnGetAutoPassanger(string ContactNo) =>
  TryCatch(async () =>
  {
      ContactNo = ContactNo == null ? "" : ContactNo;
      var oPassangers = (await _passengerService.GetDistinctPassengers()).Where(x => (x.MobileNumber).ToLower().Contains(ContactNo.ToLower())).ToList();
      return new JsonResult(oPassangers);
  });
    public Task<IActionResult> OnGetAutoAddress(string Address) =>
TryCatch(async () =>
{
    //sAddress == null ? return JsonResult(null) ;
  var oAddressList = (await _savedAddressService.GetSavedAddressByName(Address)).ToList();
    return new JsonResult(oAddressList);
});




    #endregion
}
