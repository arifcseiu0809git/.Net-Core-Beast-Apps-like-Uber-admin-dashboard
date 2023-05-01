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

namespace BEASTAdmin.UI.Pages.TransactionDetail;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public TransactionDetailModel TransactionDetail { get; set; }

    [BindProperty, DisplayName("TransactionDetail Image")]
    public IFormFile? TransactionDetailImage { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly TransactionDetailService _transactionDetailService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, TransactionDetailService transactionDetailService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._transactionDetailService = transactionDetailService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            TransactionDetail = new TransactionDetailModel();
        }
        else
        {
            TransactionDetail = await _transactionDetailService.GetTransactionDetailById(id);
        }

        //await PopulatePageElements();
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

        //if (VehicleTypeImage != null) VehicleType.ImageUrl = await UploadFile();

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        if (string.IsNullOrEmpty(TransactionDetail.Id))
        {
            TransactionDetail.CreatedBy = User.Identity.Name;
            await _transactionDetailService.InsertTransactionDetail(TransactionDetail, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            TransactionDetail.ModifiedBy = User.Identity.Name;
            await _transactionDetailService.UpdateTransactionDetail(TransactionDetail.Id, TransactionDetail, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        //await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
 //   private async Task PopulatePageElements()
 //   {
 //     ImageURL = "/images/" + (string.IsNullOrEmpty(VehicleType.ImageUrl) ? "NoImage.jpg" : "VehicleType/" + VehicleType.ImageUrl);
	//}

  //  private async Task<string> UploadFile()
  //  {
  //      string uploadFolder = "";
  //      string uniqueFileName = "";
  //      string filePath = "";

		//uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\VehicleType");

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