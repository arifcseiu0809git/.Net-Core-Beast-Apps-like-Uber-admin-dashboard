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
public partial class TransactionRequestController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<TransactionRequestController> _logger;
    private readonly IConfiguration _config;
    private readonly ITransactionRequestRepository _transactionRequestRepository;
    private readonly ICsvExporter _csvExporter;

    public TransactionRequestController(ISecurityHelper securityHelper, ILogger<TransactionRequestController> logger, IConfiguration config, ITransactionRequestRepository transactionRequestRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._transactionRequestRepository = transactionRequestRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetTransactionRequests(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _transactionRequestRepository.GetTransactionRequests(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{transactionRequestId}")]
    public Task<IActionResult> GetTransactionRequestById(string transactionRequestId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), transactionRequestId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _transactionRequestRepository.GetTransactionRequestById(transactionRequestId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, transactionRequestId));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertTransactionRequest([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        TransactionRequestModel transactionRequest = JsonSerializer.Deserialize<TransactionRequestModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transactionRequest.Id = Guid.NewGuid().ToString();

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), transactionRequest.APIEndPointRequestData))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (transactionRequest == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedTransactionRequest = await _transactionRequestRepository.InsertTransactionRequest(transactionRequest, logModel);
        return Created(nameof(GetTransactionRequestById), new { id = insertedTransactionRequest });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateTransactionRequest(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TransactionRequestModel transactionRequest = JsonSerializer.Deserialize<TransactionRequestModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transactionRequest.Id = id;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (transactionRequest == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != transactionRequest.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        await _transactionRequestRepository.UpdateTransactionRequest(transactionRequest, logModel);

        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteTransactionRequest(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        await _transactionRequestRepository.DeleteTransactionRequest(id, logModel);

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

        var result = await _transactionRequestRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}