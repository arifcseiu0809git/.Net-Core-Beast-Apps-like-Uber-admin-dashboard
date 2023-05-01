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
public partial class VehicleCurrentLocationController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<VehicleCurrentLocationController> _logger;
    private readonly IVehicleCurrentLocationRepository _vehicleCurrentLocationRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly IConfiguration _config;

    public VehicleCurrentLocationController(ISecurityHelper securityHelper, ILogger<VehicleCurrentLocationController> logger, IVehicleCurrentLocationRepository vehicleCurrentLocationRepository, ICsvExporter csvExporter, IConfiguration config)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._vehicleCurrentLocationRepository = vehicleCurrentLocationRepository;
        this._csvExporter = csvExporter;
        this._config = config;
    }

    [HttpGet]
    public Task<IActionResult> GetVehicleCurrentLocations(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _vehicleCurrentLocationRepository.GetVehicleCurrentLocations(pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetVehicleCurrentLocationById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _vehicleCurrentLocationRepository.GetVehicleCurrentLocationById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertVehicleCurrentLocation([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        VehicleCurrentLocationModel vehicleCurrentLocation = JsonSerializer.Deserialize<VehicleCurrentLocationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicleCurrentLocation.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicleCurrentLocation.GoingDirection))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (vehicleCurrentLocation == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

        string insertedVehicleId = await _vehicleCurrentLocationRepository.InsertVehicleCurrentLocation(vehicleCurrentLocation, logModel);
        return Created(nameof(GetVehicleCurrentLocationById), new { id = insertedVehicleId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateVehicleCurrentLocation(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        VehicleCurrentLocationModel vehicleCurrentLocation = JsonSerializer.Deserialize<VehicleCurrentLocationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (vehicleCurrentLocation == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != vehicleCurrentLocation.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var vehicleCurrentLocationToUpdate = await _vehicleCurrentLocationRepository.GetVehicleCurrentLocationById(id);
        if (vehicleCurrentLocationToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleCurrentLocationRepository.UpdateVehicleCurrentLocation(vehicleCurrentLocation, logModel);
        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteVehicleCurrentLocation(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }


        var vehicleCurrentLocationToDelete = await _vehicleCurrentLocationRepository.GetVehicleCurrentLocationById(id);
        if (vehicleCurrentLocationToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleCurrentLocationRepository.DeleteVehicleCurrentLocation(id, logModel);
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

        var result = await _vehicleCurrentLocationRepository.Export();
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}