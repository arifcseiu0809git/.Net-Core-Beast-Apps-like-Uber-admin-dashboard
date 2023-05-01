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
using BEASTAPI.Core.Constant;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PieController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PieController> _logger;
    private readonly IConfiguration _config;
    private readonly IPieRepository _pieRepository;
    private readonly ICsvExporter _csvExporter;

    public PieController(ISecurityHelper securityHelper, ILogger<PieController> logger, IConfiguration config, IPieRepository pieRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._pieRepository = pieRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetPies(int pageNumber) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0) return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _pieRepository.GetPies(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);
        #endregion

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetPieById(string id) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (id == "") return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _pieRepository.GetPieById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));
        #endregion

        return Ok(result);
    });

    [HttpGet("GetPieByCategoryId/{categoryId}")]
    public Task<IActionResult> GetPieByCategoryId(string categoryId) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), categoryId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (categoryId =="") return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, categoryId));

        var result = await _pieRepository.GetPieByCategoryId(categoryId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, categoryId));
        #endregion

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

        var result = await _pieRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);
        #endregion

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpPost]
    public Task<IActionResult> InsertPie([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PieModel pie = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PieModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        pie.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pie.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pie == null) return BadRequest(ValidationMessages.Pie_Null);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var existingPie = await _pieRepository.GetPieByName(pie.Name);
        if (existingPie != null)
            return BadRequest(ModelState);
        #endregion

        string insertedPieId = await _pieRepository.InsertPie(pie, logModel);
        return Created(nameof(GetPieById), new { id = insertedPieId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdatePie(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PieModel pie = PostData["Data"] == null ? null : JsonSerializer.Deserialize<PieModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (id == "") return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));
        if (pie == null) return BadRequest(ValidationMessages.Pie_Null);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (id != pie.Id) return BadRequest(ValidationMessages.Pie_Mismatch);

        var pieToUpdate = await _pieRepository.GetPieById(id);
        if (pieToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));
        #endregion

        await _pieRepository.UpdatePie(pie, logModel);
        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePie(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id == "") return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var pieToDelete = await _pieRepository.GetPieById(id);
        if (pieToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));
        #endregion

        await _pieRepository.DeletePie(id, logModel);
        return NoContent(); // success
    });
}