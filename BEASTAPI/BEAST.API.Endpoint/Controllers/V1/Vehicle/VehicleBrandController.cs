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
using System.Net;
using System.Data;

namespace BEASTAPI.Endpoint.Controllers.V1.Vehicle;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class VehicleBrandController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<VehicleBrandController> _logger;
	private readonly IVehicleBrandRepository _vehicleBrandRepository;
	private readonly ICsvExporter _csvExporter;
	private readonly IConfiguration _config;

	public VehicleBrandController(ISecurityHelper securityHelper, ILogger<VehicleBrandController> logger, IVehicleBrandRepository vehicleBrandRepository, ICsvExporter csvExporter, IConfiguration config)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._vehicleBrandRepository = vehicleBrandRepository;
		this._csvExporter = csvExporter;
		this._config = config;
	}

	[HttpGet]
	public Task<IActionResult> GetVehicleBrands(int pageNumber) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber <= 0)
			return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

		var result = await _vehicleBrandRepository.GetVehicleBrands(pageNumber);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

		return Ok(result);
	});

	[HttpGet("GetDistinctVehicleBrands"), AllowAnonymous]
	public Task<IActionResult> GetDistinctVehicleBrands() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _vehicleBrandRepository.GetDistinctVehicleBrands();
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id}")]
	public Task<IActionResult> GetVehicleBrandById(string id) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _vehicleBrandRepository.GetVehicleBrandById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });
	
	//[HttpGet("GetDistinctVehicleBrands")]
	//public Task<IActionResult> GetDistinctVehicleBrands() =>
	//TryCatch(async () =>
	//{
	//	#region Validation
	//	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	//	{
	//		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
	//			return Unauthorized(ValidationMessages.InvalidHash);
	//	}
	//	#endregion

	//	var result = await _vehicleBrandRepository.GetDistinctVehicleBrands();
	//	if (result == null)
	//		return NotFound(ValidationMessages.Pie_NotFoundList);

	//	return Ok(result);
	//});

    //[Authorize(Roles = "SystemAdmin")]
    [HttpPost]
    public Task<IActionResult> InsertVehicleBrand([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        VehicleBrandModel vehicleBrand = JsonSerializer.Deserialize<VehicleBrandModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        vehicleBrand.Id = Guid.NewGuid().ToString();
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), vehicleBrand.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        
        if (vehicleBrand == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

		string insertedVehicleBrandId = await _vehicleBrandRepository.InsertVehicleBrand(vehicleBrand, logModel);
		return Created(nameof(GetVehicleBrandById), new { id = insertedVehicleBrandId });
	});

	[HttpPut("Update/{id}")]
	public Task<IActionResult> UpdateVehicleBrand(string id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		VehicleBrandModel vehicleBrand = JsonSerializer.Deserialize<VehicleBrandModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (vehicleBrand == null)
			return BadRequest(ValidationMessages.Pie_Null);

		if (id != vehicleBrand.Id)
			return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

		var vehicleBrandToUpdate = await _vehicleBrandRepository.GetVehicleBrandById(id);
		if (vehicleBrandToUpdate == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

		await _vehicleBrandRepository.UpdateVehicleBrand(vehicleBrand, logModel);
		return NoContent();
	});

	[HttpPut("Delete/{id}")]
	public Task<IActionResult> DeleteVehicleBrand(string id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}


		var vehicleBrandToDelete = await _vehicleBrandRepository.GetVehicleBrandById(id);
		if (vehicleBrandToDelete == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

		await _vehicleBrandRepository.DeleteVehicleBrand(id, logModel);
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

		var result = await _vehicleBrandRepository.Export();
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});
}