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
public partial class PricingController : ControllerBase
{
    private readonly SecurityHelper _securityHelper;
    private readonly ILogger<PricingController> _logger;
    private readonly IConfiguration _config;
    private readonly IPricingRepository _pricingRepository;
    private readonly ICsvExporter _csvExporter;

    public PricingController(SecurityHelper securityHelper, ILogger<PricingController> logger, IConfiguration config, IPricingRepository pricingRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._pricingRepository = pricingRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetPricings(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _pricingRepository.GetPricings(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetPricingById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _pricingRepository.GetPricingById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpGet("GetPricingByName/{categoryid}")]
    public Task<IActionResult> GetPricingByName(string pricingName) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pricingName.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!string.IsNullOrEmpty(pricingName))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, pricingName));

        var result = await _pricingRepository.GetPricingByName(pricingName);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, pricingName));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertPricing([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PricingModel pricing = JsonSerializer.Deserialize<PricingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pricing.VehicleTypeId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pricing == null)
            return BadRequest(ValidationMessages.Pie_Null);

       //Existing check formula need clearify
        string insertedPricingId = await _pricingRepository.InsertPricing(pricing, logModel);
        return Created(nameof(GetPricingById), new { id = insertedPricingId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdatePricing(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        PricingModel pricing = JsonSerializer.Deserialize<PricingModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (pricing == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != pricing.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var pricingToUpdate = await _pricingRepository.GetPricingById(id);
        if (pricingToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _pricingRepository.UpdatePricing(pricing, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePricing(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var pricingToDelete = await _pricingRepository.GetPricingById(id);
        if (pricingToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _pricingRepository.DeletePricing(id, logModel);

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

        var result = await _pricingRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}