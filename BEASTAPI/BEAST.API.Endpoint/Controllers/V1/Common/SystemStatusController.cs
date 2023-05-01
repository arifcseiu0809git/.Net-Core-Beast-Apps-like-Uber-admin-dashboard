using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEAST.Core.Model;
using BEAST.Core.Model.Common;
using BEAST.Infrastructure;
using System;
using System.Threading.Tasks;
using BEAST.API.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEAST.Core.Contract.Infrastructure;
using BEAST.Core.Contract.Persistence.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEAST.API.Endpoint.Controllers.V1.Common;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class SystemStatusController : ControllerBase
{
    private readonly SecurityHelper _securityHelper;
    private readonly ILogger<SystemStatusController> _logger;
    private readonly IConfiguration _config;
    private readonly ISystemStatusRepository _systemStatusRepository;
    private readonly ICsvExporter _csvExporter;

    public SystemStatusController(SecurityHelper securityHelper, ILogger<SystemStatusController> logger, IConfiguration config, ISystemStatusRepository systemStatusRepository, ICsvExporter csvExporter)
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


    [HttpGet("{id:int}")]
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


    [HttpGet("GetSystemStatusByName/{name:string}")]
    public Task<IActionResult> GetSystemStatusByName(string systemStatusName) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), systemStatusName.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!string.IsNullOrEmpty(systemStatusName))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, systemStatusName));

        var result = await _systemStatusRepository.GetSystemStatusByName(systemStatusName);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, systemStatusName));

        return Ok(result);
    });


    [HttpPost, Authorize(Roles = "SystemAdmin")]
    public Task<IActionResult> InsertSystemStatus([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        SystemStatusModel systemStatus = JsonSerializer.Deserialize<SystemStatusModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        systemStatus.Id = new Guid().ToString();

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

        int insertedSystemStatusId = await _systemStatusRepository.InsertSystemStatus(systemStatus, logModel);
        return Created(nameof(GetSystemStatusById), new { id = insertedSystemStatusId });
    });


    [HttpPut("Update/{id:int}"), Authorize(Roles = "SystemAdmin")]
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

        if (id != systemStatus.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var systemStatusToUpdate = await _systemStatusRepository.GetSystemStatusById(id);
        if (systemStatusToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _systemStatusRepository.UpdateSystemStatus(systemStatus, logModel);

        return NoContent(); // success
    });


    [HttpPut("Delete/{id:int}"), Authorize(Roles = "SystemAdmin")]
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