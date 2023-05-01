using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using BEASTAPI.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Constant;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PaymentMethodController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PaymentMethodController> _logger;
    private readonly IConfiguration _config;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ICsvExporter _csvExporter;

    public PaymentMethodController(ISecurityHelper securityHelper, ILogger<PaymentMethodController> logger, IConfiguration config, IPaymentMethodRepository paymentMethodRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._paymentMethodRepository = paymentMethodRepository;
        this._csvExporter = csvExporter;
    }

     [HttpGet]
    public Task<IActionResult> GetPaymentMethods(int pageNumber) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber < 0)
            return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, pageNumber));
        #endregion

        var result = await _paymentMethodRepository.GetPaymentMethods(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(result);
    });
   
    [HttpGet("{id}")]
    public Task<IActionResult> GetPaymentMethodById(string id) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (id == "")
            return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        var result = await _paymentMethodRepository.GetPaymentMethodById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));

        return Ok(result);
    });



    [HttpPost("InsertPaymentMethod")]
    public Task<IActionResult> InsertPaymentMethod([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PaymentMethodModel PaymentMethod = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PaymentMethodModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        PaymentMethod.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), PaymentMethod.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (PaymentMethod == null) return BadRequest(ValidationMessages.Address_NotFoundList);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
      
        #endregion

        string result = await _paymentMethodRepository.InsertPaymentMethod(PaymentMethod, logModel);
        //return Created(nameof(GetPaymentMethodById), new { id = insertedPaymentMethodId });
		return Ok(result); // success
	});

    [HttpPut("Update/{PaymentMethodId}")]
    public Task<IActionResult> UpdatePaymentMethod(string PaymentMethodId, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PaymentMethodModel PaymentMethod = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PaymentMethodModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), PaymentMethodId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (PaymentMethodId == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, PaymentMethodId));
        if (PaymentMethod == null) return BadRequest(ValidationMessages.Address_Mismatch);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (PaymentMethodId != PaymentMethod.Id) return BadRequest(ValidationMessages.Address_NotFoundList);

        var PaymentMethodToUpdate = await _paymentMethodRepository.GetPaymentMethodById(PaymentMethodId);
        if (PaymentMethodToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, PaymentMethodId));
        #endregion

      string result =  await _paymentMethodRepository.UpdatePaymentMethod(PaymentMethod, logModel);
        
		return Ok(result); // success
	});

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePaymentMethod(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, id));
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var PaymentMethodToDelete = await _paymentMethodRepository.GetPaymentMethodById(id);
        if (PaymentMethodToDelete == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        await _paymentMethodRepository.DeletePaymentMethod(id, logModel);
        return NoContent();
    });


    [HttpGet("GetPaymentMethodByuserId/{paymentTypeId}/{paymentOptionId}/{userId}")]
    public Task<IActionResult> GetPaymentMethodByuserId(string paymentTypeId, string paymentOptionId, string userId) =>
TryCatch(async () =>
{
    #region Validation
    if (Convert.ToBoolean(_config["Hash:HashChecking"]))
    {
        if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentTypeId))
            return Unauthorized(ValidationMessages.InvalidHash);
    }

    if (string.IsNullOrEmpty(paymentTypeId) || string.IsNullOrEmpty(paymentOptionId) || string.IsNullOrEmpty(userId))
        return BadRequest(String.Format(ValidationMessages.Payment_Method_NotFound));
    #endregion

    var result = await _paymentMethodRepository.GetPaymentMethodByuserId(paymentTypeId, paymentOptionId, userId);
    if (result == null)
        return NotFound(ValidationMessages.Payment_Method_NotFound);

    return Ok(result);
});


    [HttpGet("Filter/{PaymentType}/{PaymentOption}/{ContactNo}/{AccountNo}")]
    public Task<IActionResult> Filter(string PaymentType, string PaymentOption, string ContactNo, string AccountNo) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), PaymentType + PaymentOption + ContactNo + AccountNo.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (string.IsNullOrEmpty(PaymentType) && string.IsNullOrEmpty(PaymentOption) && string.IsNullOrEmpty(ContactNo) && string.IsNullOrEmpty(AccountNo))
                return BadRequest(String.Format(ValidationMessages.Invalid_Filtering, "Select Valid Filtering"));

            var result = await _paymentMethodRepository.Filter(PaymentType, PaymentOption, ContactNo, AccountNo);
            if (result == null)
                return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

            return Ok(result);
        });



	[HttpGet("Export/{PaymentType}/{PaymentOption}/{ContactNo}/{AccountNo}")]
	public Task<IActionResult> Export(string PaymentType, string PaymentOption, string ContactNo, string AccountNo) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _paymentMethodRepository.Filter(PaymentType, PaymentOption, ContactNo, AccountNo);
		if (result == null)
			return NotFound(ValidationMessages.Address_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});
}