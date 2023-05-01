using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using BEASTAdmin.UI.Areas.Identity.Data;

namespace BEASTAdmin.UI.Pages.PaymentMethod;

[Authorize(Roles = "SystemAdmin")]
public partial class PaymentMethodUpsertModel : PageModel
{
    [BindProperty]
    public PaymentMethodModel paymentMethod { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public SelectList PaymentOptionSelectList { get; set; }
    public SelectList PaymentTypeSelectList { get; set; }
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<PaymentMethodUpsertModel> _logger;
    private readonly PaymentMethodService _paymentMethodService;
    private readonly PaymentTypeService _paymentTypeService;
    private readonly PaymentOptionService _paymentOptionService;
    
    

    public PaymentMethodUpsertModel(UserManager<ApplicationUser> userManager, ILogger<PaymentMethodUpsertModel> logger, PaymentMethodService paymentMethodService, PaymentTypeService paymentTypeService, PaymentOptionService paymentOptionService, PassengerService passengerService)
    {
        this._logger = logger;
        this._paymentMethodService = paymentMethodService;
        this._paymentTypeService = paymentTypeService;
        this._paymentOptionService = paymentOptionService;
		this._userManager = userManager;
	}

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
			paymentMethod = new PaymentMethodModel();
        }
        else
        {
			paymentMethod = await _paymentMethodService.GetPaymentMethodById(id);
        }

        await PopulatePageElements();
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false)
        {
            await PopulatePageElements();
            return Page();
        }
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.1";

        if (string.IsNullOrEmpty(paymentMethod.Id))
        {
			paymentMethod.CreatedBy = _userManager.GetUserId(User);
			paymentMethod.IsActive = true;
			//paymentMethod.IsDeleted = false;
           var oOutput = await _paymentMethodService.InsertPaymentMethod(paymentMethod, logModel);
            if (oOutput.Message!= "Sucess")
            {
                ErrorMessage = oOutput.Message;
            }
            else
            {
                SuccessMessage = InformationMessages.Saved;
            }
        }
        else
        {
			paymentMethod.ModifiedBy = _userManager.GetUserId(User);
			paymentMethod.ModifiedDate = DateTime.Now;
			var oOutput = await _paymentMethodService.UpdatePaymentMethod(paymentMethod.Id, paymentMethod, logModel);
			if (oOutput.Message != "Sucess")
			{
				ErrorMessage = oOutput.Message;
			}
			else
			{
				SuccessMessage = InformationMessages.Updated;
			}
		}
        await PopulatePageElements();
        return Page();
    });


    public Task<IActionResult> OnGetAutoUsers(string ContactNo) =>
    TryCatch(async () =>
    {
      ContactNo = ContactNo == null ? "" : ContactNo;
        var users =  _userManager.Users.Where(u => u.PhoneNumber.ToLower().Contains(ContactNo.ToLower())).ToList();
        return new JsonResult(users);
    });
	public Task<IActionResult> OnGetChangePaymentType(string paymentTypeId) =>
	 TryCatch(async () =>
	 {
		 var paymentOptions = await _paymentOptionService.GetPaymentOptionByPaymentTypeId(paymentTypeId);
		 return new JsonResult(paymentOptions);
	 });

	#region "Helper Methods"

	private async Task PopulatePageElements()
    {
        PaymentTypeSelectList = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name));
        PaymentOptionSelectList = new SelectList(await _paymentOptionService.GetDistinctPaymentOptions(), nameof(PaymentOptionModel.Id), nameof(PaymentOptionModel.Name));
       
    }

    #endregion
}