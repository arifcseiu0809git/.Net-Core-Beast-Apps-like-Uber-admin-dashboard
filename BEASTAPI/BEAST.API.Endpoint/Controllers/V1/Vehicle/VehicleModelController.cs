using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Vehicle;
using Microsoft.AspNetCore.Authorization;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Infrastructure;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Endpoint.Resources;
using BEAST.API.Persistence;

namespace BEASTAPI.Endpoint.Controllers.V1.Vehicle;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class VehicleModelController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<VehicleModelController> _logger;
    private readonly IVehicleModelRepository _vehicleModelRepository;
    private readonly ICsvExporter _csvExporter;
    private readonly IConfiguration _config;

    public VehicleModelController(ISecurityHelper securityHelper, ILogger<VehicleModelController> logger, IVehicleModelRepository vehicleModelRepository, ICsvExporter csvExporter, IConfiguration config)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._vehicleModelRepository = vehicleModelRepository;
        this._csvExporter = csvExporter;
        this._config = config;
    }

    [HttpGet]
    public Task<IActionResult> GetVehicleModels(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _vehicleModelRepository.GetVehicleModels(pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });
	[HttpGet("GetDistinctVehicleModels"), AllowAnonymous]
	public Task<IActionResult> GetDistinctVehicleModels() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
        }

        #endregion

		var result = await _vehicleModelRepository.GetDistinctVehicleModels();
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(result);
	});
	[HttpGet("{id}")]
    public Task<IActionResult> GetVehicleModelById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _vehicleModelRepository.GetVehicleModelById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertVehicleModel([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        VehicleModel vehicleModel = JsonSerializer.Deserialize<VehicleModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicleModel.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicleModel.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (vehicleModel == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

        string insertedVehicleModelId = await _vehicleModelRepository.InsertVehicleModel(vehicleModel, logModel);
        return Created(nameof(GetVehicleModelById), new { id = insertedVehicleModelId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateVehicleModel(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        VehicleModel vehicleModel = JsonSerializer.Deserialize<VehicleModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (vehicleModel == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != vehicleModel.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var vehicleModelToUpdate = await _vehicleModelRepository.GetVehicleModelById(id);
        if (vehicleModelToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleModelRepository.UpdateVehicleModel(vehicleModel, logModel);
        return NoContent();
    });
    
    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteVehicleModel(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var vehicleModelToDelete = await _vehicleModelRepository.GetVehicleModelById(id);
        if (vehicleModelToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _vehicleModelRepository.DeleteVehicleModel(id, logModel);
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

        var result = await _vehicleModelRepository.Export();
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}