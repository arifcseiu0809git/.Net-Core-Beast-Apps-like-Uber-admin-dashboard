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
using BEASTAdmin.Core.Enums;
using System;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.UI.Pages.DocumentType;

[Authorize(Roles = "SystemAdmin")]
public partial class DocumentTypeUpsert : PageModel
{
    [BindProperty]
    public DocumentTypeModel DocumentType { get; set; }
	public SelectList SelectList { get; set; }
	public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<DocumentTypeUpsert> _logger;
    private readonly IConfiguration _config;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly DocumentTypeService _DocumentTypeService;


	private readonly IWebHostEnvironment _hostEnvironment;

	public DocumentTypeUpsert(UserManager<ApplicationUser> userManager, ILogger<DocumentTypeUpsert> logger, IConfiguration config, DocumentTypeService DocumentTypeService,  IWebHostEnvironment hostEnvironment)
	{
        this._logger = logger;
        this._config = config;
        this._DocumentTypeService = DocumentTypeService;
		this._hostEnvironment = hostEnvironment;
		this._userManager= userManager;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        var oEnumDocumentFors = Enum.GetValues(typeof(DocumentFor)).Cast<DocumentFor>().Select(v => new SelectListItem
        {
            Text = v.ToString(),
            Value = ((int)v).ToString()
        }).Where(x=>Convert.ToInt16(x.Value)!=0).ToList();

		SelectList = new SelectList(oEnumDocumentFors, "Value", "Text");
		//new SelectList(await _DocumentTypeService.GetDistinctVehicleBrands(), nameof(DocumentFor.), nameof(VehicleBrandModel.Name));
		if (string.IsNullOrEmpty(id))
        {
			DocumentType = new DocumentTypeModel();
        }
        else
        {
			DocumentType = await _DocumentTypeService.GetDocumentTypeById(id);
        }
        
		//  await PopulatePageElements();
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

        if (string.IsNullOrEmpty(DocumentType.Id))
        {
			DocumentType.CreatedBy = _userManager.GetUserId(User);
			await _DocumentTypeService.InsertDocumentType(DocumentType, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
			DocumentType.ModifiedBy = _userManager.GetUserId(User);
			await _DocumentTypeService.UpdateDocumentType(DocumentType.Id, DocumentType, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

       // await PopulatePageElements();
        return Page();
    });


}