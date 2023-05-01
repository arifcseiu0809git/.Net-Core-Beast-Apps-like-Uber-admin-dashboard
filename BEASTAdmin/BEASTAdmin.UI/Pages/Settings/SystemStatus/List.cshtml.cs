using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;

namespace BEASTAdmin.UI.Pages.Settings.SystemStatus;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
	public List<SystemStatusModel> SystemStatuss { get; set; }

	// Pagination
	[BindProperty(SupportsGet = true)]
	public int PageNumber { get; set; } = 1;
	public int TotalRecords { get; set; }
	public int TotalPages { get; set; }

	private readonly ILogger<ListModel> _logger;
	private readonly SystemStatusService _SystemStatusService;

	public ListModel(ILogger<ListModel> logger, SystemStatusService SystemStatusService)
	{
		this._logger = logger;
		this._SystemStatusService = SystemStatusService;
	}

	public Task<IActionResult> OnGet() =>
	TryCatch(async () =>
	{
		var paginatedList = await _SystemStatusService.GetSystemStatus(PageNumber);
		SystemStatuss = paginatedList.Items;
		TotalRecords = paginatedList.TotalRecords;
		TotalPages = paginatedList.TotalPages;
		return Page();
	});

	public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
	    var exportFile = await _SystemStatusService.Export();
	    return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });

}