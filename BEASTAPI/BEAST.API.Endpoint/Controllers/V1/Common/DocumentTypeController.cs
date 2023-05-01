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
using Microsoft.IdentityModel.Tokens;

namespace BEASTAPI.Endpoint.Controllers.V1.Common;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class DocumentTypeController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<DocumentTypeController> _logger;
    private readonly IConfiguration _config;
    private readonly IDocumentTypeRepository _DocumentTypeRepository;
    
    private readonly ICsvExporter _csvExporter;

    public DocumentTypeController(ISecurityHelper securityHelper, ILogger<DocumentTypeController> logger, IConfiguration config, IDocumentTypeRepository DocumentTypeRepository,  ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._DocumentTypeRepository = DocumentTypeRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetDocumentTypes(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _DocumentTypeRepository.GetDocumentTypes(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetDocumentTypeById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _DocumentTypeRepository.GetDocumentTypeById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });




	[HttpPost]
    public Task<IActionResult> InsertDocumentType([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
       {
        DocumentTypeModel DocumentType = JsonSerializer.Deserialize<DocumentTypeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        DocumentType.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), DocumentType.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (DocumentType == null)
            return BadRequest(ValidationMessages.Pie_Null);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        string insertedDocumentTypeId = await _DocumentTypeRepository.InsertDocumentType(DocumentType, logModel);
        return Created(nameof(GetDocumentTypeById), new { id = insertedDocumentTypeId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateDocumentType(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        DocumentTypeModel DocumentType = JsonSerializer.Deserialize<DocumentTypeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (DocumentType == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != DocumentType.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var DocumentTypeToUpdate = await _DocumentTypeRepository.GetDocumentTypeById(id);
        if (DocumentTypeToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _DocumentTypeRepository.UpdateDocumentType(DocumentType, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteDocumentType(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var DocumentTypeToDelete = await _DocumentTypeRepository.GetDocumentTypeById(id);
        if (DocumentTypeToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _DocumentTypeRepository.DeleteDocumentType(id, logModel);

        return NoContent(); // success
    });


}