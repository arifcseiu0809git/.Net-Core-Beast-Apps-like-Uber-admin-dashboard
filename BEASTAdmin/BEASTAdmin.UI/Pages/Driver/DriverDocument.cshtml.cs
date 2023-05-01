using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using BEASTAdmin.UI.Areas.Identity.Data;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DriverDocument : PageModel
{
	public List<DocumentModel> DocumentModels { get; set; }
	[BindProperty]
	public DocumentModel DocumentModel { get; set; }
	[BindProperty]
	public bool IsApproved { get; set; } = true;
	public string ErrorMessage { get; set; }
	private readonly ILogger<DriverDocument> _logger;
	private readonly IConfiguration _config;
	private readonly DocumentService _DocumentService;
	
	private readonly IWebHostEnvironment _hostEnvironment;

	public DriverDocument(UserManager<ApplicationUser> userManager, ILogger<DriverDocument> logger, IConfiguration config, DocumentService DocumentService, IWebHostEnvironment hostEnvironment)
	{
		this._logger = logger;
		this._config = config;
		this._DocumentService = DocumentService;
		
		this._hostEnvironment = hostEnvironment;
	}

	
	public Task<IActionResult> OnGet(string id, bool IsApproved) =>
	TryCatch(async () =>
	{
		this.IsApproved = IsApproved; //for menu tracking
		DocumentModels = await _DocumentService.GetDocumentByUserId(id);

		return Page();
	});

	//public Task<IActionResult> OnPost(string id) =>
	//TryCatch(async () =>
	//{
	//	LogModel logModel = new LogModel();
	//	logModel.UserName = User.Identity.Name;
	//	logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
	//	logModel.IP = Utility.GetIPAddress(Request);

	//	var Driver = await _DocumentService.GetDocumentModelById(id);
	//	await _DocumentService.DeleteDocumentModel(id, logModel);
	//	return RedirectToPage("/Driver/DriverDocument", new { c = "Driver", p = false });
	//});

}