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
using BEASTAPI.Persistence;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PaymentOptionController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PaymentOptionController> _logger;
    private readonly IConfiguration _config;
    private readonly IPaymentOptionRepository _paymentOptionRepository;
    private readonly ICsvExporter _csvExporter;

    public PaymentOptionController(ISecurityHelper securityHelper, ILogger<PaymentOptionController> logger, IConfiguration config, IPaymentOptionRepository paymentOptionRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._paymentOptionRepository = paymentOptionRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]

    public Task<IActionResult> GetPaymentOption(int pageNumber) =>
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

        var result = await _paymentOptionRepository.GetPaymentOption(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(result);
    });

    [HttpGet("GetDistinctPaymentOptions")]
    public Task<IActionResult> GetDistinctPaymentOptions() =>
   TryCatch(async () =>
   {
       #region Validation
       if (Convert.ToBoolean(_config["Hash:HashChecking"]))
       {
           if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
               return Unauthorized(ValidationMessages.InvalidHash);
       }
       #endregion

       var result = await _paymentOptionRepository.GetDistinctPaymentOptions();
       if (result == null)
           return NotFound(ValidationMessages.Category_NotFoundList);

       return Ok(result);
   });



    [HttpGet("{id}")]
    public Task<IActionResult> GetPaymentOptionById(string id) =>
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

        var result = await _paymentOptionRepository.GetPaymentOptionById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));

        return Ok(result);
    });

    [HttpGet("Export")]
    public Task<IActionResult> Export() =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        #endregion

        var result = await _paymentOptionRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpPost("InsertPaymentOption")]
    public Task<IActionResult> InsertPaymentOption([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PaymentOptionModel paymentOption = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PaymentOptionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        paymentOption.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentOption.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (paymentOption == null) return BadRequest(ValidationMessages.Address_NotFoundList);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var existingpaymentOption = await _paymentOptionRepository.GetPaymentOptionByName(paymentOption.Name);
        if (existingpaymentOption != null)
            return BadRequest(String.Format(ValidationMessages.Category_Duplicate, paymentOption.Name));

        #endregion

        string insertedPaymentOptionId = await _paymentOptionRepository.InsertPaymentOption(paymentOption, logModel);
        return Created(nameof(GetPaymentOptionById), new { id = insertedPaymentOptionId });
    });

    [HttpPut("Update/{paymentOptionId}")]
    public Task<IActionResult> UpdatePaymentOption(string paymentOptionId, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PaymentOptionModel paymentOption = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PaymentOptionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentOptionId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (paymentOptionId == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, paymentOptionId));
        if (paymentOption == null) return BadRequest(ValidationMessages.Address_Mismatch);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (paymentOptionId != paymentOption.Id) return BadRequest(ValidationMessages.Address_NotFoundList);

        var PaymentOptionToUpdate = await _paymentOptionRepository.GetPaymentOptionById(paymentOptionId);
        if (PaymentOptionToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, paymentOptionId));
        #endregion

        await _paymentOptionRepository.UpdatePaymentOption(paymentOption, logModel);
        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePaymentOption(string id, [FromBody] LogModel logModel) =>
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

        var PaymentOptionToDelete = await _paymentOptionRepository.GetPaymentOptionById(id);
        if (PaymentOptionToDelete == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        await _paymentOptionRepository.DeletePaymentOption(id, logModel);
        return NoContent();
    });
}