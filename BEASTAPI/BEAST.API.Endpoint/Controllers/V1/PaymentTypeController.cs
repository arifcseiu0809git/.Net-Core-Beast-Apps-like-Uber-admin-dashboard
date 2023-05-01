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
using System.IO.Pipelines;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PaymentTypeController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PaymentTypeController> _logger;
    private readonly IConfiguration _config;
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly IPaymentOptionRepository _paymentOptionRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository; 

    public PaymentTypeController(
        ISecurityHelper securityHelper,
        ILogger<PaymentTypeController> logger,
        IConfiguration config,
        IPaymentTypeRepository paymentTypeRepository,
        ICsvExporter csvExporter,
        IPaymentOptionRepository paymentOptionRepository,
        IPaymentMethodRepository paymentMethodRepository)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._paymentTypeRepository = paymentTypeRepository;
        this._csvExporter = csvExporter;
        this._paymentOptionRepository = paymentOptionRepository;
        _paymentMethodRepository = paymentMethodRepository;
    }

    [HttpGet]
    public Task<IActionResult> GetPaymentTypes(int pageNumber) =>
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

        var result = await _paymentTypeRepository.GetPaymentTypes(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(result);
    });

    [HttpGet("GetDistinctPaymentTypes")]
    public Task<IActionResult> GetDistinctPaymentTypes() =>
  TryCatch(async () =>
  {
      #region Validation
      if (Convert.ToBoolean(_config["Hash:HashChecking"]))
      {
          if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
              return Unauthorized(ValidationMessages.InvalidHash);
      }
      #endregion

      var result = await _paymentTypeRepository.GetDistinctPaymentTypes();
      if (result == null)
          return NotFound(ValidationMessages.Category_NotFoundList);

      return Ok(result);
  });

    [HttpGet("{id}")]
    public Task<IActionResult> GetPaymentTypeById(string id) =>
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

        var result = await _paymentTypeRepository.GetPaymentTypeById(id);
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

        var result = await _paymentTypeRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpPost("InsertPaymentType")]
    public Task<IActionResult> InsertPaymentType([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PaymentTypeModel paymentType = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PaymentTypeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        paymentType.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentType.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (paymentType == null) return BadRequest(ValidationMessages.Address_NotFoundList);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var existingPaymentType = await _paymentTypeRepository.GetPaymentTypeByName(paymentType.Name);
        if (existingPaymentType != null)
            return BadRequest(ModelState);

        #endregion

        string insertedPaymentTypeId = await _paymentTypeRepository.InsertPaymentType(paymentType, logModel);
        return Created(nameof(GetPaymentTypeById), new { id = insertedPaymentTypeId });
    });

    [HttpPut("Update/{paymentTypeId}")]
    public Task<IActionResult> UpdatePaymentType(string paymentTypeId, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PaymentTypeModel paymentType = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PaymentTypeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentTypeId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (paymentTypeId == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, paymentTypeId));
        if (paymentType == null) return BadRequest(ValidationMessages.Address_Mismatch);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (paymentTypeId != paymentType.Id) return BadRequest(ValidationMessages.Address_NotFoundList);

        var paymentTypeToUpdate = await _paymentTypeRepository.GetPaymentTypeById(paymentTypeId);
        if (paymentTypeToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, paymentTypeId));

        //var existingPaymentType = await _paymentTypeRepository.GetPaymentTypeByName(paymentType.Name);
        //if (existingPaymentType == null)
        //    return BadRequest(ModelState);

        #endregion

        await _paymentTypeRepository.UpdatePaymentType(paymentType, logModel);
        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePaymentType(string id, [FromBody] LogModel logModel) =>
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

        var PaymentTypeToDelete = await _paymentTypeRepository.GetPaymentTypeById(id);
        if (PaymentTypeToDelete == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        await _paymentTypeRepository.DeletePaymentType(id, logModel);
        return NoContent();
    });

    [HttpGet("{paymentTypeId}/PaymentOptions")]
    public Task<IActionResult> GetPaymentOptionsByPaymentType(string paymentTypeId) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentTypeId))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(paymentTypeId))
            return BadRequest(String.Format(ValidationMessages.Payment_Options_NotFound));
        #endregion

        var result = await _paymentOptionRepository.GetPaymentOptionsByPaymentTypeId(paymentTypeId);
        if (result == null)
            return NotFound(ValidationMessages.Payment_Options_NotFound);

        return Ok(result);
    });

    [HttpGet("{paymentTypeId}/PaymentOption/{paymentOptionId}/PaymentMethods")]
    public Task<IActionResult> GetPaymentMethodsByPaymentTypeAndPaymentOption(string paymentTypeId, string paymentOptionId) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), paymentTypeId))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(paymentTypeId) || string.IsNullOrEmpty(paymentOptionId))
            return BadRequest(String.Format(ValidationMessages.Payment_Method_NotFound));
        #endregion

        var result = await _paymentMethodRepository.GetPaymentMethodsByPaymentTypeAndPaymentOption(paymentTypeId, paymentOptionId);
        if (result == null)
            return NotFound(ValidationMessages.Payment_Method_NotFound);

        return Ok(result);
    });

    [HttpGet("CheckDuplicate")]
    public Task<IActionResult> CheckDuplicate(string id, string name) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        #endregion

        var result = await _paymentTypeRepository.CheckIfDuplicateExists(id, name);

        return Ok(result);
    });
}