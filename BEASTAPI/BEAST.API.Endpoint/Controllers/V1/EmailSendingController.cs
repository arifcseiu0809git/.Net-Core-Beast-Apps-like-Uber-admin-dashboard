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
public partial class EmailSendingController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<EmailSendingController> _logger;
    private readonly IConfiguration _config;
    private readonly IEmailSendingRepository _emailSendingRepository;
    private readonly ICsvExporter _csvExporter;

    public EmailSendingController(ISecurityHelper securityHelper, ILogger<EmailSendingController> logger, IConfiguration config, IEmailSendingRepository emailSendingRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._emailSendingRepository = emailSendingRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetEmailSendings(int pageNumber) =>
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

        var result = await _emailSendingRepository.GetEmailSendings(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(result);
    });
   
    [HttpGet("{id}")]
    public Task<IActionResult> GetEmailSendingById(string id) =>
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

        var result = await _emailSendingRepository.GetEmailSendingById(id);
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

        var result = await _emailSendingRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpPost("InsertEmailSending")]
    public Task<IActionResult> InsertEmailSending([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        EmailSendingModel emailSending = PostData["Data"] == null ? null : JsonSerializer.Deserialize<EmailSendingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        emailSending.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), emailSending.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (emailSending == null) return BadRequest(ValidationMessages.Address_NotFoundList);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
      
        #endregion

        string insertedEmailSendingId = await _emailSendingRepository.InsertEmailSending(emailSending, logModel);
        return Created(nameof(GetEmailSendingById), new { id = insertedEmailSendingId });
    });

    [HttpPut("Update/{emailSendingId}")]
    public Task<IActionResult> UpdateEmailSending(string emailSendingId, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        EmailSendingModel emailSending = PostData["Data"] == null ? null : JsonSerializer.Deserialize<EmailSendingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), emailSendingId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (emailSendingId == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, emailSendingId));
        if (emailSending == null) return BadRequest(ValidationMessages.Address_Mismatch);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (emailSendingId != emailSending.Id) return BadRequest(ValidationMessages.Address_NotFoundList);

        var EmailSendingToUpdate = await _emailSendingRepository.GetEmailSendingById(emailSendingId);
        if (EmailSendingToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, emailSendingId));
        #endregion

        await _emailSendingRepository.UpdateEmailSending(emailSending, logModel);
        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteEmailSending(string id, [FromBody] LogModel logModel) =>
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

        var EmailSendingToDelete = await _emailSendingRepository.GetEmailSendingById(id);
        if (EmailSendingToDelete == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        await _emailSendingRepository.DeleteEmailSending(id, logModel);
        return NoContent();
    });
}