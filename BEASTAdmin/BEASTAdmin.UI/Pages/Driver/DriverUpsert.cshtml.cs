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
using Microsoft.AspNetCore.Identity;
using BEASTAdmin.UI.Areas.Identity.Data;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DriverUpsert: PageModel
{
	[BindProperty]
	public DriverModel DriverModel { get; set; }

	[BindProperty, DisplayName("Date of Birth")]
	public string DateOfBirth { get; set; }

	public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<DriverUpsert> _logger;
    private readonly IConfiguration _config;
    private readonly DriverModelService _DriverModelService;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public DriverUpsert(UserManager<ApplicationUser> userManager, ILogger<DriverUpsert> logger, IConfiguration config, DriverModelService DriverModelService,  CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._userManager = userManager;
		this._DriverModelService = DriverModelService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
		if (string.IsNullOrEmpty(id))
        {
			DriverModel = new DriverModel();
        }
        else
        {
			DriverModel = await _DriverModelService.GetDriverModelById(id);
        }
        
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

        DriverModel.IsApproved = false;
        DriverModel.ApprovedBy = "";

		if (string.IsNullOrEmpty(DriverModel.Id))
        {
            DriverModel.CreatedBy = _userManager.GetUserId(User);  //User.Identity.ge;
			var user = new ApplicationUser
			{
				UserName = DriverModel.Email,
				UserType = "EA5AAC9C-79C6-4C5F-823E-FB22AF7B76E5",
				FullName = DriverModel.FirstName,
				Email = DriverModel.Email,
				PhoneNumber = DriverModel.MobileNumber,
				IsActive = true,
				IsDeleted = false,
				CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now
            };

			var result = await _userManager.CreateAsync(user, "123456789");
			var Newuser = await _userManager.FindByNameAsync(user.UserName);
			DriverModel.UserId = Newuser.Id;  //"bc3bc813-ebd6-4090-aed5-4a0317f9e4e0";//need solution (for test purpose entry)
            DriverModel.IsApproved = false; //default
			await _DriverModelService.InsertDriverMOdel(DriverModel, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
			DriverModel.ModifiedBy = _userManager.GetUserId(User);
			await _DriverModelService.UpdateDriverModel(DriverModel.Id, DriverModel, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

       // await PopulatePageElements();
        return Page();
    });


}