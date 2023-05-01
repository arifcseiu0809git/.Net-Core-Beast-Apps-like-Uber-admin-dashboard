using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.SavedAddress;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<SavedAddressModel> savedAddress { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly SavedAddressService _savedAddressService;

    public ListModel(ILogger<ListModel> logger, SavedAddressService savedAddressService)
    {
        this._logger = logger;
        this._savedAddressService = savedAddressService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _savedAddressService.GetSavedAddresses(PageNumber);
		savedAddress = paginatedList.Items;
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

        await _savedAddressService.DeleteSavedAddress(id, logModel);
        return RedirectToPage("/SavedAddress/List", new { c = "cat", p = "catl" });
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _savedAddressService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}