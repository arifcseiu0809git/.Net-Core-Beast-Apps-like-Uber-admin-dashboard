using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Infrastructure;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Endpoint.Resources;
using BEASTAPI.Core.Contract.Vehicle;

namespace BEASTAPI.Endpoint.Controllers.V1.Vehicle;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class VehicleFareController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<VehicleFareController> _logger;
    private readonly IVehicleFareRepository _vehicleFareRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly IConfiguration _config;

    public VehicleFareController(ISecurityHelper securityHelper, ILogger<VehicleFareController> logger, IVehicleFareRepository vehicleFareRepository, ICsvExporter csvExporter, IConfiguration config)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._vehicleFareRepository = vehicleFareRepository;
        this._csvExporter = csvExporter;
        this._config = config;
    }

    [HttpGet]
    public Task<IActionResult> GetVehicleFares(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _vehicleFareRepository.GetVehicleFares(pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetVehicleFareById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _vehicleFareRepository.GetVehicleFareById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertVehicleFare([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        VehicleFareModel vehicleFare = JsonSerializer.Deserialize<VehicleFareModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicleFare.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicleFare.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (vehicleFare == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

        string insertedVehicleId = await _vehicleFareRepository.InsertVehicleFare(vehicleFare, logModel);
        return Created(nameof(GetVehicleFareById), new { id = insertedVehicleId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateVehicleFare(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        VehicleFareModel vehicleFare = JsonSerializer.Deserialize<VehicleFareModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (vehicleFare == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != vehicleFare.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var vehicleFareToUpdate = await _vehicleFareRepository.GetVehicleFareById(id);
        if (vehicleFareToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleFareRepository.UpdateVehicleFare(vehicleFare, logModel);
        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteVehicleFare(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var vehicleFareToDelete = await _vehicleFareRepository.GetVehicleFareById(id);
        if (vehicleFareToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleFareRepository.DeleteVehicleFare(id, logModel);
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

        var result = await _vehicleFareRepository.Export();
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}