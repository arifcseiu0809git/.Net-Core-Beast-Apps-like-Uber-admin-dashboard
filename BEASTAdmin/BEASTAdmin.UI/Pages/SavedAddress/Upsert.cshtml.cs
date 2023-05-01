using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.SavedAddress;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public SavedAddressModel savedAddress { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly SavedAddressService _savedAddressService;

    public UpsertModel(ILogger<UpsertModel> logger, SavedAddressService savedAddressService)
    {
        this._logger = logger;
        this._savedAddressService = savedAddressService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
			savedAddress = new SavedAddressModel();
        }
        else
        {
			savedAddress = await _savedAddressService.GetSavedAddressById(id);
        }

        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false) return Page();

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.1";

        if (string.IsNullOrEmpty(savedAddress.Id))
        {
			savedAddress.CreatedBy = User.Identity.Name;
			savedAddress.IsActive = true;
			savedAddress.IsDeleted = false;
            await _savedAddressService.InsertSavedAddress(savedAddress, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
			savedAddress.ModifiedBy = User.Identity.Name;
			savedAddress.ModifiedDate = DateTime.Now;
            await _savedAddressService.UpdateSavedAddress(savedAddress.Id, savedAddress, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        return Page();
    });
}