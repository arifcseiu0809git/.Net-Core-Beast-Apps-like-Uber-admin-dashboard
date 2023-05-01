using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.Settings.EmailTemplate;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public EmailTemplateModel EmailTemplate { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly EmailTemplateService _emailTemplateService;

    public UpsertModel(ILogger<UpsertModel> logger, EmailTemplateService emailTemplateService)
    {
        this._logger = logger;
        this._emailTemplateService = emailTemplateService;
    }

    public Task<IActionResult> OnGet(int id) =>
    TryCatch(async () =>
    {
        EmailTemplate = await _emailTemplateService.GetEmailTemplateById(id);
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
        EmailTemplate.ModifiedBy = User.Identity.Name;
        await _emailTemplateService.UpdateEmailTemplate(EmailTemplate.Id, EmailTemplate, logModel);
        SuccessMessage = InformationMessages.Updated;

        return Page();
    });
}