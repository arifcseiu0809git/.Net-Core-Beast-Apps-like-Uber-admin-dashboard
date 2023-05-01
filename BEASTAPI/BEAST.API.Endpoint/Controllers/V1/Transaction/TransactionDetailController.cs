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
public partial class TransactionDetailController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<TransactionDetailController> _logger;
    private readonly IConfiguration _config;
    private readonly ITransactionDetailRepository _transactionDetailRepository;
    private readonly ICsvExporter _csvExporter;

    public TransactionDetailController(ISecurityHelper securityHelper, ILogger<TransactionDetailController> logger, IConfiguration config, ITransactionDetailRepository transactionDetailRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._transactionDetailRepository = transactionDetailRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetTransactionDetails(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _transactionDetailRepository.GetTransactionDetails(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{transactionDetailId}")]
    public Task<IActionResult> GetTransactionDetailById(string transactionDetailId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), transactionDetailId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _transactionDetailRepository.GetTransactionDetailById(transactionDetailId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, transactionDetailId));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertTransactionDetail([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        TransactionDetailModel transactionDetail = JsonSerializer.Deserialize<TransactionDetailModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transactionDetail.Id = Guid.NewGuid().ToString();

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(),Convert.ToString(transactionDetail.TransactionAmount)))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (transactionDetail == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedTransactionDetail = await _transactionDetailRepository.InsertTransactionDetail(transactionDetail, logModel);
        return Created(nameof(GetTransactionDetailById), new { id = insertedTransactionDetail });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateTransactionDetail(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TransactionDetailModel transactionDetail = JsonSerializer.Deserialize<TransactionDetailModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        transactionDetail.Id = id;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (transactionDetail == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != transactionDetail.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        await _transactionDetailRepository.UpdateTransactionDetail(transactionDetail, logModel);

        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteTransactionDetail(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        await _transactionDetailRepository.DeleteTransactionDetail(id, logModel);

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

        var result = await _transactionDetailRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}