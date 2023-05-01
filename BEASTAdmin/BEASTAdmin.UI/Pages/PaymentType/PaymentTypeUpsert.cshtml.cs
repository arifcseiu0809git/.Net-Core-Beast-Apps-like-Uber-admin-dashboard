using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BEASTAdmin.UI.Pages.PaymentType;

[Authorize(Roles = "SystemAdmin")]
public partial class PaymentTypeUpsertModel : PageModel
{
    [BindProperty]
    public PaymentTypeModel paymentType { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<PaymentTypeUpsertModel> _logger;
    private readonly PaymentTypeService _paymentTypeService; 

    public PaymentTypeUpsertModel(ILogger<PaymentTypeUpsertModel> logger, PaymentTypeService paymentTypeService)
    {
        this._logger = logger;
        this._paymentTypeService = paymentTypeService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
			paymentType = new PaymentTypeModel();
        }
        else
        {
			paymentType = await _paymentTypeService.GetPaymentTypeById(id);
        }
         
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (!await ValidatePost())
        {
            ErrorMessage = "Can not insert duplicate types";
            return Page();
        }

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.1";

        paymentType.Name = paymentType.Name.Trim();
        if (string.IsNullOrEmpty(paymentType.Id))
        {
			paymentType.CreatedBy = User.Identity.Name;
			paymentType.IsActive = true;
			paymentType.IsDeleted = false;
            await _paymentTypeService.InsertPaymentType(paymentType, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            paymentType.ModifiedBy = User.Identity.Name;
			paymentType.ModifiedDate = DateTime.Now;
            await _paymentTypeService.UpdatePaymentType(paymentType.Id, paymentType, logModel);
            SuccessMessage = InformationMessages.Updated;
        }
         
        return Page();
    });

   
}