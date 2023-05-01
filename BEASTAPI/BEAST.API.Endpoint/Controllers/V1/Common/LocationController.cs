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

namespace BEASTAPI.Endpoint.Controllers.V1.Common;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class LocationController : ControllerBase
{
    private readonly SecurityHelper _securityHelper;
    private readonly ILogger<LocationController> _logger;
    private readonly IConfiguration _config;
    private readonly ILocationRepository _locationRepository;
    private readonly ICsvExporter _csvExporter;

    public LocationController(SecurityHelper securityHelper, ILogger<LocationController> logger, IConfiguration config, ILocationRepository locationRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._locationRepository = locationRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetLocations(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _locationRepository.GetLocations(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetLocationById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _locationRepository.GetLocationById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpGet("GetLocationByName/{categoryId}")]
    public Task<IActionResult> GetLocationByName(string locationName) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), locationName.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!string.IsNullOrEmpty(locationName))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, locationName));

        var result = await _locationRepository.GetLocationByName(locationName);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, locationName));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertLocation([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        LocationModel location = JsonSerializer.Deserialize<LocationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), location.LandmarkName))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (location == null)
            return BadRequest(ValidationMessages.Pie_Null);

        var existingLocation = await _locationRepository.GetLocationByName(location.LandmarkName);
        if (existingLocation != null)
        {
            ModelState.AddModelError("Duplicate Location", String.Format(ValidationMessages.Pie_Duplicate, location.LandmarkName));
            return BadRequest(ModelState);
        }

        string insertedLocationId = await _locationRepository.InsertLocation(location, logModel);
        return Created(nameof(GetLocationById), new { id = insertedLocationId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateLocation(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        LocationModel location = JsonSerializer.Deserialize<LocationModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (location == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != location.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var locationToUpdate = await _locationRepository.GetLocationById(id);
        if (locationToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _locationRepository.UpdateLocation(location, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteLocation(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var locationToDelete = await _locationRepository.GetLocationById(id);
        if (locationToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _locationRepository.DeleteLocation(id, logModel);

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

        var result = await _locationRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}