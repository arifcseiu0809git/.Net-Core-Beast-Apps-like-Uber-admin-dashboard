using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Service;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.Extensions.Hosting;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Service.Vehicle;

using BEASTAdmin.Core.Model.Common;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using BEASTAdmin.UI.Areas.Identity.Data;

namespace BEASTAdmin.UI.Pages.ManualRideBooking;

[Authorize(Roles = "SystemAdmin")]
public partial class InvoiceDetailsModel : PageModel
{
    [BindProperty]
    public TripInitialModel TripInitial { get; set; }
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public SelectList PaymentTypeSelectList { get; set; }
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<InvoiceDetailsModel> _logger;
    private readonly IConfiguration _config;
    private readonly TripInitialService _tripInitialService;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly PaymentTypeService _paymentTypeService;
    private readonly PaymentOptionService _paymentOptionService;
    private readonly PaymentMethodService _paymentMethodService;
    public InvoiceDetailsModel(UserManager<ApplicationUser> userManager,  ILogger<InvoiceDetailsModel> logger, IConfiguration config, PaymentTypeService paymentTypeService,  TripInitialService tripInitialService,  IWebHostEnvironment hostEnvironment, PaymentOptionService paymentOptionService,PaymentMethodService paymentMethodService)
	{
        this._logger = logger;
        this._config = config;
        this._tripInitialService = tripInitialService;
        this._hostEnvironment = hostEnvironment;
        _paymentTypeService = paymentTypeService;
        _paymentOptionService = paymentOptionService;
        _paymentMethodService = paymentMethodService;
        this._userManager = userManager;
    }

    public Task<IActionResult> OnGet(string Id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(Id))
        {
            TripInitial = new TripInitialModel();
        }
        else
        {
            TripInitial = await _tripInitialService.GetTripInitialById(Id);
        }
        await GetPaymentTypes();
        return Page();
    });

    protected async Task GetPaymentTypes()
    {
        PaymentTypeSelectList = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name));
    }

    public Task<IActionResult> OnGetChangePaymentType(string paymentTypeId) =>
     TryCatch(async () =>
     {
         var paymentOptions = await _paymentOptionService.GetPaymentOptionByPaymentTypeId(paymentTypeId);
         return new JsonResult(paymentOptions);
     });

    public Task<IActionResult> OnGetChangePaymentOption(string paymentTypeId, string paymentOptionId, string userId) =>
    TryCatch(async () =>
    {
        var paymentMethods = await _paymentMethodService.GetPaymentMethodByuserId(paymentTypeId, paymentOptionId, userId);
        return new JsonResult(paymentMethods);
    });

    //$.getJSON(`?handler=AddPayment&tripInitialId=${TripInitialId}&paymentMethodId=${paymentMethodId}&paymentTypeId=${paymentTypeId}&paymentOptionId=${paymentOptionId}&accountNumber=${AccountNumber}&expireMonthYear=${ExpireMonthYear}&cvvCode=${CvvCode}&transactionAmount=${TransactionAmount}&BillDate=${BillDate}`
    public Task<IActionResult> OnGetAddPayment(string tripInitialId, string paymentMethodId, string paymentTypeId, string paymentOptionId, string accountNumber, string expireMonthYear, string cvvCode, double transactionAmount, DateTime BillDate) =>
TryCatch(async () =>
{
    LogModel logModel = new LogModel();
    logModel.UserName = User.Identity.Name;
    logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
    logModel.IP = Utility.GetIPAddress(Request);

    var tripInitialModel = new TripInitialModel();
    tripInitialModel.Id = tripInitialId;
    tripInitialModel.PaymentMethodId = paymentMethodId;
    tripInitialModel.PaymentTypeId = paymentTypeId;
    tripInitialModel.PaymentOptionId = paymentOptionId;
    tripInitialModel.AccountNumber = accountNumber;
    tripInitialModel.ExpireMonthYear= expireMonthYear;
    tripInitialModel.InitialFee = transactionAmount;
    tripInitialModel.CvvCode = cvvCode;
    tripInitialModel.RequestTime = BillDate;
    //FromDate = fromDate,
    //ToDate = toDate,
    //IsCommisionReceived = true,
    //TransactionId = transactionId,
    //CommissionReceiveDate = DateTime.Now,


    tripInitialModel.ModifiedBy = _userManager.GetUserId(User);
    tripInitialModel.ModifiedDate = DateTime.Now;
    //CommissionRate = Convert.ToDecimal(_config["AdminSetting:CommissionRate"]),
    
    

string  oresult = await _tripInitialService.MakePayment(tripInitialId, tripInitialModel, logModel);
    SuccessMessage = InformationMessages.Saved;
    if(oresult==null)
        return new JsonResult("Success");
    else
        return new JsonResult(oresult);
});
}
