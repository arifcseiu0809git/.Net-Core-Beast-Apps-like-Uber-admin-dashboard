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

namespace BEASTAdmin.UI.Pages.City;

[Authorize(Roles = "SystemAdmin")]
public partial class CityUpsertModel : PageModel
{
    [BindProperty]
    public CityModel City { get; set; }

    [BindProperty, DisplayName("City Image")]
    public IFormFile? CityImage { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }
    public SelectList CountrySelectList { get; set; }

    private readonly ILogger<CityUpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly CityService _cityService;
    private readonly CountryService _countryService;
    private readonly IWebHostEnvironment _hostEnvironment;

	public CityUpsertModel(ILogger<CityUpsertModel> logger, IConfiguration config, CityService cityService, CountryService countryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._cityService = cityService;
        this._countryService = countryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            City = new CityModel();
        }
        else
        {
            City = await _cityService.GetCityById(id);
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

        //if (VehicleTypeImage != null) VehicleType.ImageUrl = await UploadFile();

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        if (string.IsNullOrEmpty(City.Id))
        {
            City.CreatedBy = User.Identity.Name;
            await _cityService.InsertCity(City, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            City.ModifiedBy = User.Identity.Name;
            await _cityService.UpdateCity(City.Id, City, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
    private async Task PopulatePageElements()
    {
        CountrySelectList = new SelectList(await _countryService.GetDistinctCountries(), nameof(CountryModel.Id), nameof(CountryModel.CountryName));
    }

    //  private async Task<string> UploadFile()
    //  {
    //      string uploadFolder = "";
    //      string uniqueFileName = "";
    //      string filePath = "";

    //uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\Transaction");

    //// Delete existing image
    //if (!string.IsNullOrEmpty(Transaction.ImageUrl))
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