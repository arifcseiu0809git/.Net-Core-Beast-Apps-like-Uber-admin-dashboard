using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Infrastructure;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Endpoint.Resources;
using BEAST.API.Persistence;

namespace BEASTAPI.Endpoint.Controllers.V1.Driver;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class VehicleTypeController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<VehicleTypeController> _logger;
    private readonly IVehicleTypeRepository _vehicleTypeRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly IConfiguration _config;

    public VehicleTypeController(ISecurityHelper securityHelper, ILogger<VehicleTypeController> logger, IVehicleTypeRepository vehicleTypeRepository, ICsvExporter csvExporter, IConfiguration config)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._vehicleTypeRepository = vehicleTypeRepository;
        this._csvExporter = csvExporter;
        this._config = config;
    }

    [HttpGet]
    public Task<IActionResult> GetVehicleTypes(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _vehicleTypeRepository.GetVehicleTypes(pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });
	
	[HttpGet("GetDistinctVehicleTypes")]
	public Task<IActionResult> GetDistinctVehicleTypes() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
        }

        #endregion

		var result = await _vehicleTypeRepository.GetDistinctVehicleTypes();
		if (result == null)
			return NotFound(ValidationMessages.Pie_NotFoundList);

		return Ok(result);
	});
	[HttpGet("{id}")]
    public Task<IActionResult> GetVehicleTypeById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _vehicleTypeRepository.GetVehicleTypeById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    //[HttpPost, Authorize(Roles = "SystemAdmin")]
    [HttpPost]
    public Task<IActionResult> InsertVehicleType([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        VehicleTypeModel vehicleType = JsonSerializer.Deserialize<VehicleTypeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicleType.Id = Guid.NewGuid().ToString();
        vehicleType.CreatedDate = DateTime.Now;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicleType.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (vehicleType == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

        string insertedVehicleTypeId = await _vehicleTypeRepository.InsertVehicleType(vehicleType, logModel);
        return Created(nameof(GetVehicleTypeById), new { id = insertedVehicleTypeId });
    });

    //[HttpPut("Update/{id}"), Authorize(Roles = "SystemAdmin")]
    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateVehicleType(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        VehicleTypeModel vehicleType = JsonSerializer.Deserialize<VehicleTypeModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicleType.Id = id;
        vehicleType.CreatedDate = DateTime.Now;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (vehicleType == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != vehicleType.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var vehicleTypeToUpdate = await _vehicleTypeRepository.GetVehicleTypeById(id);
        if (vehicleTypeToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleTypeRepository.UpdateVehicleType(vehicleType, logModel);
        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteVehicleType(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var vehicleTypeToDelete = await _vehicleTypeRepository.GetVehicleTypeById(id);
        if (vehicleTypeToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleTypeRepository.DeleteVehicleType(id, logModel);
        return NoContent(); ;
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

        var result = await _vehicleTypeRepository.Export();
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}