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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Constant;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class SavedAddressController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<SavedAddressController> _logger;
    private readonly IConfiguration _config;
    private readonly ISavedAddressRepository _savedAddressRepository;
    private readonly ICsvExporter _csvExporter;

    public SavedAddressController(ISecurityHelper securityHelper, ILogger<SavedAddressController> logger, IConfiguration config, ISavedAddressRepository savedAddressRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._savedAddressRepository = savedAddressRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetSavedAddresses(int pageNumber) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber < 0)
            return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, pageNumber));
        #endregion

        var result = await _savedAddressRepository.GetSavedAddresses(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(result);
    });
   
    [HttpGet("{id}")]
    public Task<IActionResult> GetSavedAddressById(string id) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (id == "")
            return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        var result = await _savedAddressRepository.GetSavedAddressById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));

        return Ok(result);
    });

    //[HttpGet("GetDistinctPassengers"), AllowAnonymous]
    [HttpGet("GetSavedAddressByName/{addressname}")]
    public Task<IActionResult> GetSavedAddressByName(string addressname) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), addressname.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (addressname == "")
            return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, addressname));
        #endregion

        var result = await _savedAddressRepository.GetSavedAddressByName(addressname);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, addressname));

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
        #endregion

        var result = await _savedAddressRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Address_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpPost("InsertSavedAddress")]
    public Task<IActionResult> InsertSavedAddress([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        SavedAddressModel savedAddress = PostData["Data"] == null ? null : JsonSerializer.Deserialize<SavedAddressModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        savedAddress.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), savedAddress.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (savedAddress == null) return BadRequest(ValidationMessages.Address_NotFoundList);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
      
        #endregion

        string insertedSavedAddressId = await _savedAddressRepository.InsertSavedAddress(savedAddress, logModel);
        return Created(nameof(GetSavedAddressById), new { id = insertedSavedAddressId });
    });

    [HttpPut("Update/{savedAddressId}")]
    public Task<IActionResult> UpdateSavedAddress(string savedAddressId, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        SavedAddressModel savedAddress = PostData["Data"] == null ? null : JsonSerializer.Deserialize<SavedAddressModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), savedAddressId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (savedAddressId == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, savedAddressId));
        if (savedAddress == null) return BadRequest(ValidationMessages.Address_Mismatch);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (savedAddressId != savedAddress.Id) return BadRequest(ValidationMessages.Address_NotFoundList);

        var SavedAddressToUpdate = await _savedAddressRepository.GetSavedAddressById(savedAddressId);
        if (SavedAddressToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, savedAddressId));
        #endregion

        await _savedAddressRepository.UpdateSavedAddress(savedAddress, logModel);
        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteSavedAddress(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id == "") return BadRequest(String.Format(ValidationMessages.Address_NotFoundList, id));
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var SavedAddressToDelete = await _savedAddressRepository.GetSavedAddressById(id);
        if (SavedAddressToDelete == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundList, id));
        #endregion

        await _savedAddressRepository.DeleteSavedAddress(id, logModel);
        return NoContent();
    });
}