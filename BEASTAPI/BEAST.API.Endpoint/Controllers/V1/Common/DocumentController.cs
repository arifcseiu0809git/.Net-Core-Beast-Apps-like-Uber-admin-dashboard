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
public partial class DocumentController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<DocumentController> _logger;
    private readonly IConfiguration _config;
    private readonly IDocumentRepository _documentRepository;
    
    private readonly ICsvExporter _csvExporter;

    public DocumentController(ISecurityHelper securityHelper, ILogger<DocumentController> logger, IConfiguration config, IDocumentRepository documentRepository,  ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._documentRepository = documentRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetDocuments(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _documentRepository.GetDocuments(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetDocumentById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _documentRepository.GetDocumentById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });



	[HttpGet("GetDocumentsByUserId/{userId}")]

	public Task<IActionResult> GetDocumentsByUserId(string userId) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userId.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (string.IsNullOrEmpty(userId))
			return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, userId));

		var result = await _documentRepository.GetDocumentsByUserId(userId);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, userId));

		return Ok(result);
	});



	[HttpPost]
    public Task<IActionResult> InsertDocument([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
       {
        DocumentModel document = JsonSerializer.Deserialize<DocumentModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        document.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), document.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (document == null)
            return BadRequest(ValidationMessages.Pie_Null);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        string insertedDocumentId = await _documentRepository.InsertDocument(document, logModel);
        return Created(nameof(GetDocumentById), new { id = insertedDocumentId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateDocument(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        DocumentModel document = JsonSerializer.Deserialize<DocumentModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (document == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != document.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var documentToUpdate = await _documentRepository.GetDocumentById(id);
        if (documentToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _documentRepository.UpdateDocument(document, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteDocument(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var documentToDelete = await _documentRepository.GetDocumentById(id);
        if (documentToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _documentRepository.DeleteDocument(id, logModel);

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

        var result = await _documentRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });


}