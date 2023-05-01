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
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class CityController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<CityController> _logger;
    private readonly IConfiguration _config;
    private readonly ICityRepository _cityRepository;
    private readonly ICsvExporter _csvExporter;

    public CityController(ISecurityHelper securityHelper, ILogger<CityController> logger, IConfiguration config, ICityRepository cityRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._cityRepository = cityRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetCities(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _cityRepository.GetCities(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{cityId}")]
    public Task<IActionResult> GetCityById(string cityId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), cityId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _cityRepository.GetCityById(cityId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, cityId));

        return Ok(result);
    });

    [HttpGet("GetDistinctCities"), AllowAnonymous]
    public Task<IActionResult> GetDistinctCities() =>
   TryCatch(async () =>
   {
       #region Validation
       if (Convert.ToBoolean(_config["Hash:HashChecking"]))
       {
           if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
               return Unauthorized(ValidationMessages.InvalidHash);
       }
       #endregion

       var result = await _cityRepository.GetDistinctCities();
       if (result == null)
           return NotFound(ValidationMessages.Pie_NotFoundCategoryId);

       return Ok(result);
   });

    [HttpGet("GetCityByName/{name}")]
    public Task<IActionResult> GetCityByName(string name) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), name.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(name))
            return BadRequest(String.Format(ValidationMessages.Pie_NotFoundId, name));

        var result = await _cityRepository.GetCityByName(name);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, name));

        return Ok(result);
    });

    [HttpGet("GetCityByCuntryId/{Id}")]
    public Task<IActionResult> GetCityByCuntryId(string Id) =>
TryCatch(async () =>
{
    if (Convert.ToBoolean(_config["Hash:HashChecking"]))
    {
        if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Id.ToString()))
            return Unauthorized(ValidationMessages.InvalidHash);
    }

    if (string.IsNullOrEmpty(Id))
        return BadRequest(String.Format(ValidationMessages.Pie_NotFoundId, Id));

    var result = await _cityRepository.GetCityByCuntryId(Id);
    if (result == null)
        return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, Id));

    return Ok(result);
});

    [HttpPost]
    public Task<IActionResult> InsertCity([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        CityModel city = JsonSerializer.Deserialize<CityModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        city.Id = Guid.NewGuid().ToString();

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Convert.ToString(city.Name)))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (city == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedCity = await _cityRepository.InsertCity(city, logModel);
        return Created(nameof(GetCityById), new { id = insertedCity });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateCity(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        CityModel city = JsonSerializer.Deserialize<CityModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        city.Id = id;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (city == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != city.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        await _cityRepository.UpdateCity(city, logModel);

        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteCity(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        await _cityRepository.DeleteCity(id, logModel);

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

        var result = await _cityRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}