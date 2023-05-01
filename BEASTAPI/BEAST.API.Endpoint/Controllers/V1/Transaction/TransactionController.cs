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
public partial class TransactionController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<TransactionController> _logger;
    private readonly IConfiguration _config;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICsvExporter _csvExporter;

    public TransactionController(ISecurityHelper securityHelper, ILogger<TransactionController> logger, IConfiguration config, ITransactionRepository transactionRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._transactionRepository = transactionRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetTransactions(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _transactionRepository.GetTransactions(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{transactionId}")]
    public Task<IActionResult> GetTransactionById(string transactionId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), transactionId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _transactionRepository.GetTransactionById(transactionId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, transactionId));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertTransaction([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        TransactionModel transaction = JsonSerializer.Deserialize<TransactionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transaction.Id = Guid.NewGuid().ToString();

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), Convert.ToString(transaction.TotalBillAmount)))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (transaction == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedTransaction = await _transactionRepository.InsertTransaction(transaction, logModel);
        return Created(nameof(GetTransactionById), new { id = insertedTransaction });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateTransaction(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TransactionModel transaction = JsonSerializer.Deserialize<TransactionModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transaction.Id = id;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (transaction == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != transaction.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        await _transactionRepository.UpdateTransaction(transaction, logModel);

        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteTransaction(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        await _transactionRepository.DeleteTransaction(id, logModel);

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

        var result = await _transactionRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}