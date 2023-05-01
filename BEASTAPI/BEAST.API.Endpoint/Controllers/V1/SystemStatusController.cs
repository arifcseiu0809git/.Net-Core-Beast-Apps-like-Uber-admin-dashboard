using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using BEASTAPI.Infrastructure;
using System;
using System.Threading.Tasks;
using BEASTAPI.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class SystemStatusController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<SystemStatusController> _logger;
    private readonly IConfiguration _config;
    private readonly ISystemStatusRepository _systemStatusRepository;
    private readonly ICsvExporter _csvExporter;

    public SystemStatusController(ISecurityHelper securityHelper, ILogger<SystemStatusController> logger, IConfiguration config, ISystemStatusRepository systemStatusRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._systemStatusRepository = systemStatusRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetSystemStatus(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _systemStatusRepository.GetSystemStatus(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

	[HttpGet("GetDistinctSystemStatus")]
	public Task<IActionResult> GetDistinctSystemStatus() =>
    TryCatch(async () =>
    {
	    #region Validation
	    if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	    {
		    if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
			    return Unauthorized(ValidationMessages.InvalidHash);
	    }
	    #endregion

	    var result = await _systemStatusRepository.GetDistinctSystemStatus();
	    if (result == null)
		    return NotFound(ValidationMessages.Pie_NotFoundList);

	    return Ok(result);
    });
	[HttpGet("{id}")]
    public Task<IActionResult> GetSystemStatusById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }


        var result = await _systemStatusRepository.GetSystemStatusById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });
        
    [HttpGet("GetSystemStatusByName/{name}")]
    public Task<IActionResult> GetSystemStatusByName(string name) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), name.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(name))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, name));

        var result = await _systemStatusRepository.GetSystemStatusByName(name);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, name));

        return Ok(result);
    });


    [HttpPost]
    public Task<IActionResult> InsertSystemStatus([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        SystemStatusModel systemStatus = JsonSerializer.Deserialize<SystemStatusModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        systemStatus.Id = Guid.NewGuid().ToString();

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), systemStatus.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (systemStatus == null)
            return BadRequest(ValidationMessages.Pie_Null);

        var existingSystemStatus = await _systemStatusRepository.GetSystemStatusByName(systemStatus.Name);
        if (existingSystemStatus != null)
        {
            ModelState.AddModelError("Duplicate SystemStatus", String.Format(ValidationMessages.Pie_Duplicate, systemStatus.Name));
            return BadRequest(ModelState);
        }

        string insertedSystemStatusId = await _systemStatusRepository.InsertSystemStatus(systemStatus, logModel);
        return Created(nameof(GetSystemStatusById), new { id = insertedSystemStatusId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateSystemStatus(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        SystemStatusModel systemStatus = JsonSerializer.Deserialize<SystemStatusModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (systemStatus == null)
            return BadRequest(ValidationMessages.Pie_Null);

        var systemStatusToUpdate = await _systemStatusRepository.GetSystemStatusById(id);
        if (systemStatusToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        systemStatus.Id = systemStatusToUpdate.Id;
        await _systemStatusRepository.UpdateSystemStatus(systemStatus, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteSystemStatus(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }


        var systemStatusToDelete = await _systemStatusRepository.GetSystemStatusById(id);
        if (systemStatusToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _systemStatusRepository.DeleteSystemStatus(id, logModel);

        return NoContent(); // success
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

        var result = await _systemStatusRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}