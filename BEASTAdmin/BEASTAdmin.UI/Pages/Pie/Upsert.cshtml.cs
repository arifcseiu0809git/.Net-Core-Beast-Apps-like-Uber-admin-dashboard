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

namespace BEASTAdmin.UI.Pages.Pie;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public PieModel Pie { get; set; }

    [BindProperty, DisplayName("Expiry Date")]
    public string ExpiryDate { get; set; }

    [BindProperty, DisplayName("Pie Image")]
    public IFormFile? PieImage { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }
    public SelectList CategorySelectList { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly PieService _pieService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, PieService pieService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._pieService = pieService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            Pie = new PieModel();
        }
        else
        {
            Pie = await _pieService.GetPieById(id);
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

        if (PieImage != null) Pie.ImageUrl = await UploadFile();
        Pie.ExpiryDate = dateExpiryDate;

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.12";//Utility.GetIPAddress(Request);

        if (string.IsNullOrEmpty(Pie.Id))
        {
            Pie.CreatedBy = User.Identity.Name;
            Pie.CreatedDate = DateTime.Now;
            Pie.IsActive = true;
            Pie.IsDeleted = false;
            await _pieService.InsertPie(Pie, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            Pie.ModifiedBy = User.Identity.Name;
            await _pieService.UpdatePie(Pie.Id, Pie, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
    private async Task PopulatePageElements()
    {
        CategorySelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));
		ImageURL = "/images/" + (string.IsNullOrEmpty(Pie.ImageUrl) ? "NoImage.jpg" : "pie/" + Pie.ImageUrl);
	}

    private async Task<string> UploadFile()
    {
        string uploadFolder = "";
        string uniqueFileName = "";
        string filePath = "";

		uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\pie");

		// Delete existing image
		if (!string.IsNullOrEmpty(Pie.ImageUrl))
            System.IO.File.Delete(Path.Combine(uploadFolder, Pie.ImageUrl));

        // Upload new image
        uniqueFileName = Guid.NewGuid().ToString() + "-" + PieImage.FileName;
        filePath = Path.Combine(uploadFolder, uniqueFileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await PieImage.CopyToAsync(fileStream);
        }

        return uniqueFileName;
    }
    #endregion
}