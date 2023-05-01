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

namespace BEASTAdmin.UI.Pages.Transaction;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public TransactionModel Transaction { get; set; }

    [BindProperty, DisplayName("Transaction Image")]
    public IFormFile? TransactionImage { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    //public string ImageURL { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, TransactionService transactionService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._transactionService = transactionService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            Transaction = new TransactionModel();
        }
        else
        {
            Transaction = await _transactionService.GetTransactionById(id);
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

        if (string.IsNullOrEmpty(Transaction.Id))
        {
            Transaction.CreatedBy = User.Identity.Name;
            await _transactionService.InsertTransaction(Transaction, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            Transaction.ModifiedBy = User.Identity.Name;
            await _transactionService.UpdateTransaction(Transaction.Id, Transaction, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        //await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
 //   private async Task PopulatePageElements()
 //   {
 //     ImageURL = "/images/" + (string.IsNullOrEmpty(VehicleType.ImageUrl) ? "NoImage.jpg" : "Transaction/" + Transaction.ImageUrl);
	//}

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