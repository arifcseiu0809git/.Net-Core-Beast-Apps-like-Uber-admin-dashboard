using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Service.Vehicle;

namespace BEASTAdmin.UI.Pages.DocumentType;

[Authorize(Roles = "SystemAdmin")]
public partial class DocumentTypeList: PageModel
{
    public List<DocumentTypeModel> DocumentTypes { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<DocumentTypeList> _logger;
    private readonly IConfiguration _config;
    private readonly DocumentTypeService _DocumentTypeService;
    private readonly CategoryService _categoryService;
	private readonly IWebHostEnvironment _hostEnvironment;

	public DocumentTypeList(ILogger<DocumentTypeList> logger, IConfiguration config, DocumentTypeService DocumentTypeService, CategoryService categoryService, IWebHostEnvironment hostEnvironment)
    {
        this._logger = logger;
        this._config = config;
        this._DocumentTypeService = DocumentTypeService;
        this._categoryService = categoryService;
		this._hostEnvironment = hostEnvironment;
	}

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _DocumentTypeService.GetDocumentTypes(PageNumber);
		DocumentTypes = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;

        return Page();
    });

    public Task<IActionResult> OnPostDelete(string id) =>
    TryCatch(async () =>
    {
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        var vehicle = await _DocumentTypeService.GetDocumentTypeById(id);
		await _DocumentTypeService.DeleteDocumentType(id, logModel);
        return RedirectToPage("/DocumentType/DocumentTypeList", new {  });
    });

	public Task<IActionResult> OnPostExport() =>
TryCatch(async () =>
{
	var exportFile = await _DocumentTypeService.Export();
	return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
});
}