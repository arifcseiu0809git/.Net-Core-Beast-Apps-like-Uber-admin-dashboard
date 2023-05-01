using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using BEASTAPI.Infrastructure;
using System;
using System.Threading.Tasks;
using BEASTAPI.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence.Transaction;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class TransactionResponseController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<TransactionResponseController> _logger;
    private readonly IConfiguration _config;
    private readonly ITransactionResponseRepository _transactionResponseRepository;
    private readonly ICsvExporter _csvExporter;

    public TransactionResponseController(ISecurityHelper securityHelper, ILogger<TransactionResponseController> logger, IConfiguration config, ITransactionResponseRepository transactionResponseRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._transactionResponseRepository = transactionResponseRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetTransactionResponses(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _transactionResponseRepository.GetTransactionResponses(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{transactionResponseId}")]
    public Task<IActionResult> GetTransactionResponseById(string transactionResponseId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), transactionResponseId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _transactionResponseRepository.GetTransactionResponseById(transactionResponseId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, transactionResponseId));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertTransactionResponse([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        TransactionResponseModel transactionResponse = JsonSerializer.Deserialize<TransactionResponseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transactionResponse.Id = Guid.NewGuid().ToString();

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), transactionResponse.APIResponseData))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (transactionResponse == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedTransactionResponse = await _transactionResponseRepository.InsertTransactionResponse(transactionResponse, logModel);
        return Created(nameof(GetTransactionResponseById), new { id = insertedTransactionResponse });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateTransactionResponse(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TransactionResponseModel transactionResponse = JsonSerializer.Deserialize<TransactionResponseModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transactionResponse.Id = id;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (transactionResponse == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != transactionResponse.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        await _transactionResponseRepository.UpdateTransactionResponse(transactionResponse, logModel);

        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteTransactionResponse(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        await _transactionResponseRepository.DeleteTransactionResponse(id, logModel);

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

        var result = await _transactionResponseRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}