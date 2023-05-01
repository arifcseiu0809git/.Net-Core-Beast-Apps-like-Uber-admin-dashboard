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

namespace BEASTAdmin.UI.Pages.Trip;

[Authorize(Roles = "SystemAdmin")]
public partial class InvoiceDetailsModel : PageModel
{
    [BindProperty]
    public TripModel Trip { get; set; }

    [BindProperty, DisplayName("Pick Up Date Time")]
    public string RequestTime { get; set; }
    [BindProperty]
    public string DriverName { get; set; } = ""; //for filtering
     [BindProperty]
    public string StatusId { get; set; } = ""; //for filtering
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }

   

    private readonly ILogger<InvoiceDetailsModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly TripService _TripService;
    private readonly IWebHostEnvironment _hostEnvironment;

	public InvoiceDetailsModel(UserManager<ApplicationUser> userManager, ILogger<InvoiceDetailsModel> logger, IConfiguration config, TripService TripService,  IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._TripService = TripService;
        this._hostEnvironment = hostEnvironment;
        this._userManager = userManager;

    }

    public Task<IActionResult> OnGet(string statusId,  string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            Trip = new TripModel();
        }
        else
        {
            Trip = await _TripService.GetTripById(id);
        }
        StatusId = statusId;

        return Page();
    });


    
}
