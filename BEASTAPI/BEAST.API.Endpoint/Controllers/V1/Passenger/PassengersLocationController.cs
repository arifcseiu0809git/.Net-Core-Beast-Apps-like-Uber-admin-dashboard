using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Contract;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;
using BEASTAPI.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using BEASTAPI.Endpoint.Resources;

namespace BEASTAPI.Endpoint.Controllers.V1.Passenger;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PassengersLocationController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PassengersLocationController> _logger;
    private readonly IConfiguration _config;
    private readonly IPassengersLocationRepository _passengerLocationRepository;
    private readonly ICsvExporter _csvExporter;

    public PassengersLocationController(ISecurityHelper securityHelper, ILogger<PassengersLocationController> logger, IConfiguration config, IPassengersLocationRepository passengerLocationRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._passengerLocationRepository = passengerLocationRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetPassengersLocations(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _passengerLocationRepository.GetPassengersLocations(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetPassengersLocationById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _passengerLocationRepository.GetPassengersLocationById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertPassengersLocation([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PassengersLocationModel passengerLocation = JsonSerializer.Deserialize<PassengersLocationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passengerLocation.LandmarkName))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (passengerLocation == null)
            return BadRequest(ValidationMessages.Pie_Null);

        var existingPassengerLocation = await _passengerLocationRepository.GetPassengersLocationByName(passengerLocation.LandmarkName);
        if (existingPassengerLocation != null)
        {
            ModelState.AddModelError("Duplicate Passenger Location", String.Format(ValidationMessages.Pie_Duplicate, passengerLocation.LandmarkName));
            return BadRequest(ModelState);
        }

        string insertedPassengerLocationId = await _passengerLocationRepository.InsertPassengersLocation(passengerLocation, logModel);
        return Created(nameof(GetPassengersLocationById), new { id = insertedPassengerLocationId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdatePassengersLocation(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        PassengersLocationModel passengerLocation = JsonSerializer.Deserialize<PassengersLocationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (passengerLocation == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != passengerLocation.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var passengerLocationToUpdate = await _passengerLocationRepository.GetPassengersLocationById(id);
        if (passengerLocationToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _passengerLocationRepository.UpdatePassengersLocation(passengerLocation, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePassengersLocation(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var passengerLocationToDelete = await _passengerLocationRepository.GetPassengersLocationById(id);
        if (passengerLocationToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _passengerLocationRepository.DeletePassengersLocation(id, logModel);

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

        var result = await _passengerLocationRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}