using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BEASTAdmin.UI.Pages.PaymentOption;

[Authorize(Roles = "SystemAdmin")]
public partial class PaymentOptionUpsertModel : PageModel
{
    [BindProperty]
    public PaymentOptionModel paymentOption { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public SelectList PaymentTypeSelectList { get; set; }

    private readonly ILogger<PaymentOptionUpsertModel> _logger;
    private readonly PaymentOptionService _paymentOptionService;
    private readonly PaymentTypeService _paymentTypeService;

    public PaymentOptionUpsertModel(ILogger<PaymentOptionUpsertModel> logger, PaymentOptionService paymentOptionService, PaymentTypeService paymentTypeService)
    {
        this._logger = logger;
        this._paymentOptionService = paymentOptionService;
        this._paymentTypeService = paymentTypeService; 
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
			paymentOption = new PaymentOptionModel();
        }
        else
        {
			paymentOption = await _paymentOptionService.GetPaymentOptionById(id);
        }

        await PopulatePageElements();
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false)
        { 
            return Page();
        }

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.1";

        if (string.IsNullOrEmpty(paymentOption.Id))
        {
			paymentOption.CreatedBy = User.Identity.Name;
			paymentOption.IsActive = true;
			paymentOption.IsDeleted = false;
            await _paymentOptionService.InsertPaymentOption(paymentOption, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
			paymentOption.ModifiedBy = User.Identity.Name;
			paymentOption.ModifiedDate = DateTime.Now;
            await _paymentOptionService.UpdatePaymentOption(paymentOption.Id, paymentOption, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"

    private async Task PopulatePageElements()
    {
        PaymentTypeSelectList = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name)); 
    }

    #endregion
}