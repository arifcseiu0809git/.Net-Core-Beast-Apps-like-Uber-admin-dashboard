using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using BEASTAPI.Infrastructure;
using System;
using System.Threading.Tasks;
using BEASTAPI.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class SMSController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PieController> _logger;
    private readonly IConfiguration _config;
    private readonly ISMSRepository _smsRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly ISMSSender _smsSender;

    public SMSController(ISecurityHelper securityHelper, ILogger<PieController> logger, IConfiguration config, ISMSRepository smsRepository, ICsvExporter csvExporter, ISMSSender smsSender)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._smsRepository = smsRepository;
        this._csvExporter = csvExporter;
        this._smsSender = smsSender;
    }

    [HttpPost("Send")]
    public Task<IActionResult> Send([FromBody] SMSModel sms) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), sms.Content))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (sms == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (sms.To.Count == 0 || sms.Content == "")
            return BadRequest(ValidationMessages.EmailTemplate_Null1);
        #endregion

        await _smsSender.SendSMS(sms);
        return NoContent();
    });

    [HttpGet]
    public Task<IActionResult> GetSMSes(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _smsRepository.GetSMSes(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{smsId}")]
    public Task<IActionResult> GetSMSById(string smsId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), smsId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _smsRepository.GetSMSById(smsId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, smsId));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertSMS([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        SMSModels sms = JsonSerializer.Deserialize<SMSModels>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        sms.Id = Guid.NewGuid().ToString();
        sms.CreatedDate = DateTime.Now;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), sms.BodyText))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (sms == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedSMS = await _smsRepository.InsertSMS(sms, logModel);
        return Created(nameof(GetSMSById), new { id = insertedSMS });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateSMS(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        SMSModels sms = JsonSerializer.Deserialize<SMSModels>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        sms.Id = id;
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (sms == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != sms.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        await _smsRepository.UpdateSMS(sms, logModel);

        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteSMS(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        await _smsRepository.DeleteSMS(id, logModel);

        return NoContent();
    });

    [HttpGet("Export")]
    public Task<IActionResult> Export() =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _smsRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}