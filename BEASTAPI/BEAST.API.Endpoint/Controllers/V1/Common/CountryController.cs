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
public partial class CountryController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<CountryController> _logger;
    private readonly IConfiguration _config;
    private readonly ICountryRepository _countryRepository;
    private readonly ICsvExporter _csvExporter;

    public CountryController(ISecurityHelper securityHelper, ILogger<CountryController> logger, IConfiguration config, ICountryRepository countryRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._countryRepository = countryRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetCountries(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _countryRepository.GetCountries(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetCountryById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _countryRepository.GetCountryById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpGet("GetDistinctCountries")]
    public Task<IActionResult> GetDistinctCountries() =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        #endregion

        var result = await _countryRepository.GetDistinctCountries();
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundId);

        return Ok(result);
    });

    [HttpGet("GetCountryByName/{name}")]
    public Task<IActionResult> GetCountryByName(string name) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), name.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(name))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, name));

        var result = await _countryRepository.GetCountryByName(name);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, name));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertCountry([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        CountryModel country = JsonSerializer.Deserialize<CountryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        country.Id = Guid.NewGuid().ToString();
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), country.CountryName))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (country == null)
            return BadRequest(ValidationMessages.Pie_Null);

        var existingCountry = await _countryRepository.GetCountryByName(country.CountryName);
        if (existingCountry != null)
        {
            ModelState.AddModelError("Duplicate Country", String.Format(ValidationMessages.Pie_Duplicate, country.CountryName));
            return BadRequest(ModelState);
        }

        string insertedCountryId = await _countryRepository.InsertCountry(country, logModel);
        return Created(nameof(GetCountryById), new { id = insertedCountryId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateCountry(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        CountryModel country = JsonSerializer.Deserialize<CountryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (country == null)
            return BadRequest(ValidationMessages.Pie_Null);

        var countryToUpdate = await _countryRepository.GetCountryById(id);
        if (id != countryToUpdate.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        if (countryToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        country.Id = id;
        await _countryRepository.UpdateCountry(country, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteCountry(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var countryToDelete = await _countryRepository.GetCountryById(id);
        if (countryToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _countryRepository.DeleteCountry(id, logModel);

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

        var result = await _countryRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}