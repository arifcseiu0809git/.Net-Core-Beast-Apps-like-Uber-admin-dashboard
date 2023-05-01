using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Resources;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DriverCommissionTransaction : PageModel
{
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public DriverModel Driver { get; set; }
    public List<DriverCommissionModel> DriverTripCommissions { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public decimal Revenues { get; set; }
    public decimal CommissionRate { get { return 20.00m; } }
    public SelectList PaymentTypeSelectList { get; set; }

    private readonly ILogger<DriverCommissionTransaction> _logger;
    private readonly IConfiguration _config;
    private readonly DriverModelService _driverModelService;
    private readonly AdminEarningService _adminEarningService;
    private readonly PaymentTypeService _paymentTypeService;
    private readonly PaymentOptionService _paymentOptionService;
    private readonly PaymentMethodService _paymentMethodService;

    public DriverCommissionTransaction(
        ILogger<DriverCommissionTransaction> logger,
        IConfiguration config,
        DriverModelService driverModelService,
        AdminEarningService adminEarningService,
        PaymentTypeService paymentTypeService,
        PaymentOptionService paymentOptionService,
        PaymentMethodService paymentMethodService)
    {
        this._logger = logger;
        this._config = config;
        this._driverModelService = driverModelService;
        this._adminEarningService = adminEarningService;
        _paymentTypeService = paymentTypeService;
        _paymentOptionService = paymentOptionService;
        _paymentMethodService = paymentMethodService;
    }

    public Task<IActionResult> OnGet(string driverId, int pagenumber) =>
    TryCatch(async () =>
    {
        PageNumber = pagenumber;
        if (string.IsNullOrEmpty(driverId))
        {
            return NotFound($"Unable to load driver commissions with driver ID '{driverId}'.");
        }
        else
        {
            Driver = await _driverModelService.GetDriverModelById(driverId);
            await PopulatePageElements();
        }
        return Page();
    });

    private async Task PopulatePageElements()
    {
        await GetDriverTransactions(PageNumber);
        await GetPaymentTypes();
    }

    protected async Task GetDriverTransactions(int pageNo)
    {
        var paginatedList = await _driverModelService.GetDriverCommission(Driver.Id, pageNo);
        DriverTripCommissions = paginatedList.Items ?? new List<DriverCommissionModel>();
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
    }

    protected async Task GetPaymentTypes()
    {
        PaymentTypeSelectList = new SelectList(await _paymentTypeService.GetDistinctPaymentTypes(), nameof(PaymentTypeModel.Id), nameof(PaymentTypeModel.Name));
    }

    public Task<IActionResult> OnGetDueCommissions(string driverId, DateTime fromDate, DateTime toDate) =>
        TryCatch(async () =>
        {
            var dueAmount = await _adminEarningService.GetDueCommissionByDriverId(driverId, fromDate, toDate);

            return new JsonResult(dueAmount);
        });

    public Task<IActionResult> OnGetChangePaymentType(string paymentTypeId) =>
        TryCatch(async () =>
        {
            var paymentOptions = await _paymentOptionService.GetPaymentOptionByPaymentTypeId(paymentTypeId);
            return new JsonResult(paymentOptions);
        });

    public Task<IActionResult> OnGetChangePaymentOption(string paymentTypeId, string paymentOptionId) =>
    TryCatch(async () =>
    {
        var paymentMethods = await _paymentMethodService.GetPaymentMethodByPaymentTypeAndPaymentOption(paymentTypeId, paymentOptionId);
        return new JsonResult(paymentMethods);
    });

    //[ValidateAntiForgeryToken]
    public Task<IActionResult> OnGetAddTransaction(string driverId, DateTime fromDate, DateTime toDate, 
        string paymentTypeId, string paymentOptionId, string paymentMethodId, string transactionId) =>
    TryCatch(async () =>
    {
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        var adminEarning = new AdminEarningInsertModel
        {
            DriverId = driverId,
            FromDate = fromDate,
            ToDate = toDate,
            IsCommisionReceived = true,
            TransactionId = transactionId,
            CommissionReceiveDate = DateTime.Now,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = User.Identity.Name,
            CreatedDate = DateTime.Now,
            ModifiedBy = User.Identity.Name,
            ModifiedDate = DateTime.Now,
            CommissionRate = Convert.ToDecimal(_config["AdminSetting:CommissionRate"]),
            PaymentTypeId = paymentTypeId,
            PaymentOptionId = paymentOptionId,
            PaymentMethodId = paymentMethodId
        };
        await _adminEarningService.AddDriverCommissions(adminEarning, logModel);
        SuccessMessage = InformationMessages.Saved;
        return new JsonResult("Success");
    });
}