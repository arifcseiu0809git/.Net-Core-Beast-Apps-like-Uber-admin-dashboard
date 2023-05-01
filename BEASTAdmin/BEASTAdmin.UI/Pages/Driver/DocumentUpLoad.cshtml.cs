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
using BEASTAdmin.Core.Model.Common;
using MongoDB.Driver.Linq;
using BEASTAdmin.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DocumentUpLoad: PageModel
{
	[BindProperty]
	public DocumentModel DocumentModel { get; set; }

	public string ImageURL { get; set; }
	[BindProperty, DisplayName("Document Image")]
	public IFormFile? DocumentImage { get; set; }
	public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<DriverStatus> _logger;
    private readonly IConfiguration _config;
    private readonly DocumentService _DocumentService;
    private readonly SystemStatusService _SystemStatusService;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public DocumentUpLoad(UserManager<ApplicationUser> userManager, ILogger<DriverStatus> logger, IConfiguration config, DocumentService DocumentService, SystemStatusService SystemStatusService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._DocumentService = DocumentService;
        this._SystemStatusService = SystemStatusService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
		this._userManager = userManager;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
		DocumentModel = await _DocumentService.GetDocumentById(id);
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

		if (DocumentImage != null) DocumentModel.DocumentUrl = await UploadFile();
		

		LogModel logModel = new LogModel();
		logModel.UserName = User.Identity.Name;
		logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
		logModel.IP = "192.168.1.12";//Utility.GetIPAddress(Request);

		DocumentModel.ModifiedBy = _userManager.GetUserId(User);
		await _DocumentService.UpdateDocument(DocumentModel.Id, DocumentModel, logModel);
		SuccessMessage = InformationMessages.Updated;
	

		await PopulatePageElements();
		return Page();
	});

	#region "Helper Methods"
	private async Task PopulatePageElements()
	{
		//CategorySelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));
		ImageURL = "/images/" + (string.IsNullOrEmpty(DocumentModel.DocumentUrl) ? "NoImage.jpg" : "doc/"+DocumentModel.Name+"/"+ DocumentModel.DocumentUrl);
	}

	private async Task<string> UploadFile()
	{
		string uploadFolder = "";
		string uniqueFileName = "";
		string filePath = "";

		uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images\\doc\\"+ DocumentModel.Name);

		// Delete existing image
		if (!string.IsNullOrEmpty(DocumentModel.DocumentUrl))
			System.IO.File.Delete(Path.Combine(uploadFolder, DocumentModel.DocumentUrl));

		// Upload new image
		uniqueFileName = Guid.NewGuid().ToString() + "-" + DocumentImage.FileName;
		filePath = Path.Combine(uploadFolder, uniqueFileName);
		using (var fileStream = new FileStream(filePath, FileMode.Create))
		{
			await DocumentImage.CopyToAsync(fileStream);
		}

		return uniqueFileName;
	}
	#endregion
}