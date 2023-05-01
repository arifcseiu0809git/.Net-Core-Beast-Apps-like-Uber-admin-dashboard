using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Vehicle;
using Microsoft.AspNetCore.Authorization;
using BEASTAPI.Core.Contract.Infrastructure;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Endpoint.Resources;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Endpoint.Controllers.V1.Vehicle;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class VehicleController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<VehicleController> _logger;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly IConfiguration _config;

    public VehicleController(ISecurityHelper securityHelper, ILogger<VehicleController> logger, IVehicleRepository vehicleRepository, ICsvExporter csvExporter, IConfiguration config)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._vehicleRepository = vehicleRepository;
        this._csvExporter = csvExporter;
        this._config = config;
    }

    [HttpGet]
    public Task<IActionResult> GetVehicles(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _vehicleRepository.GetVehicles(pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetVehicleById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _vehicleRepository.GetVehicleById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });
	
	[HttpGet("GetVehicleByTypeId/{vehicleTypeId}"), AllowAnonymous]
	public Task<IActionResult> GetVehicleByTypeId(string vehicleTypeId) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicleTypeId.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (vehicleTypeId == "") return BadRequest(String.Format(ValidationMessages.Pie_InvalidCategoryId, vehicleTypeId));

		var result = await _vehicleRepository.GetVehicleByTypeId(vehicleTypeId);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundCategoryId, vehicleTypeId));
		#endregion

		return Ok(result);
	});
	[HttpPost]
    public Task<IActionResult> InsertVehicle([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        BEASTAPI.Core.Model.Vehicle.Vehicle vehicle = JsonSerializer.Deserialize<BEASTAPI.Core.Model.Vehicle.Vehicle>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicle.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicle.RegistrationNo))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (vehicle == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

        string insertedVehicleId = await _vehicleRepository.InsertVehicle(vehicle, logModel);
        return Created(nameof(GetVehicleById), new { id = insertedVehicleId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateVehicle(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        BEASTAPI.Core.Model.Vehicle.Vehicle Vehicle = JsonSerializer.Deserialize<BEASTAPI.Core.Model.Vehicle.Vehicle>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Vehicle == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != Vehicle.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var VehicleToUpdate = await _vehicleRepository.GetVehicleById(id);
        if (VehicleToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleRepository.UpdateVehicle(Vehicle, logModel);
        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteVehicle(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var VehicleToDelete = await _vehicleRepository.GetVehicleById(id);
        if (VehicleToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleRepository.DeleteVehicle(id, logModel);
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

        var result = await _vehicleRepository.Export();
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}