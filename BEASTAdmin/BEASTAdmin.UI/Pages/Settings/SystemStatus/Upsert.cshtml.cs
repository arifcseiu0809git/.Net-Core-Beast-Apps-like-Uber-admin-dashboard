using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Common;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.Settings.SystemStatus;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public SystemStatusModel SystemStatus { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly SystemStatusService _SystemStatusService;

    public UpsertModel(ILogger<UpsertModel> logger, SystemStatusService SystemStatusService)
    {
        this._logger = logger;
        this._SystemStatusService = SystemStatusService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
		if (string.IsNullOrEmpty(id))
		{
			SystemStatus = new SystemStatusModel();
		}
		else
		{
			SystemStatus = await _SystemStatusService.GetSystemStatusById(id);
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
        logModel.IP = Utility.GetIPAddress(Request);
		if (string.IsNullOrEmpty(SystemStatus.Id))
		{
			SystemStatus.CreatedBy = User.Identity.Name;
			await _SystemStatusService.InsertSystemStatus(SystemStatus, logModel);
			SuccessMessage = InformationMessages.Saved;
		}
		else
		{
			SystemStatus.ModifiedBy = User.Identity.Name;
			await _SystemStatusService.UpdateSystemStatus(SystemStatus.Id, SystemStatus, logModel);
			SuccessMessage = InformationMessages.Updated;
		}
		return Page();
    });
}