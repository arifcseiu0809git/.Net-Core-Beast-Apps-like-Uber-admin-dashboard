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
using BEASTAPI.Core.Contract.Persistence.Map;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Endpoint.Controllers.V1.Map;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class TripInitialController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<TripInitialController> _logger;
    private readonly IConfiguration _config;
    private readonly ITripInitialRepository _tripInitialRepository;
    private readonly ICsvExporter _csvExporter;

    public TripInitialController(ISecurityHelper securityHelper, ILogger<TripInitialController> logger, IConfiguration config, ITripInitialRepository tripInitialRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._tripInitialRepository = tripInitialRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetTripInitials(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _tripInitialRepository.GetTripInitials(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetTripInitialById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _tripInitialRepository.GetTripInitialById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

	[HttpGet("Filter/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}")]
	public Task<IActionResult> Filter(string StatusId, string VehicleTypId, string DriverName, string PassengerName) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), StatusId + VehicleTypId + DriverName+ PassengerName.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (string.IsNullOrEmpty(StatusId) && string.IsNullOrEmpty(VehicleTypId) && string.IsNullOrEmpty(DriverName) && string.IsNullOrEmpty(PassengerName))
			return BadRequest(String.Format(ValidationMessages.Invalid_Filtering, "Select Valid Filtering"));

		var result = await _tripInitialRepository.Filter(StatusId, VehicleTypId, DriverName, PassengerName);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

		return Ok(result);
	});


	[HttpPost]
    public Task<IActionResult> InsertTripInitial([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        TripInitialModel tripInitial = JsonSerializer.Deserialize<TripInitialModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        tripInitial.Id = Guid.NewGuid().ToString();
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), tripInitial.OriginAddress))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (tripInitial == null)
            return BadRequest(ValidationMessages.Pie_Null);

        

        int insertedTripInitialId = await _tripInitialRepository.InsertTripInitial(tripInitial, logModel);
        return Created(nameof(GetTripInitialById), new { id = insertedTripInitialId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateTripInitial(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TripInitialModel tripInitial = JsonSerializer.Deserialize<TripInitialModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tripInitial == null)
            return BadRequest(ValidationMessages.Pie_Null);

       

        var tripInitialToUpdate = await _tripInitialRepository.GetTripInitialById(id);
        if (tripInitialToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _tripInitialRepository.UpdateTripInitial(tripInitial, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteTripInitial(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var tripInitialToDelete = await _tripInitialRepository.GetTripInitialById(id);
        if (tripInitialToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _tripInitialRepository.DeleteTripInitial(id, logModel);

        return NoContent(); // success
    });


    [HttpPut("MakePayment/{id}")]
    public Task<IActionResult> MakePayment(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TripInitialModel tripInitial = JsonSerializer.Deserialize<TripInitialModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tripInitial == null)
            return BadRequest(ValidationMessages.Pie_Null);



        var tripInitialToUpdate = await _tripInitialRepository.GetTripInitialById(id);
        if (tripInitialToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        string result =  await _tripInitialRepository.MakePayment(tripInitial, logModel);
        //if (result == null)
        //    return NotFound();
        return Ok(result); // success
    });

    [HttpGet("Export/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}")]
	public Task<IActionResult> Export(string StatusId, string VehicleTypId, string DriverName, string PassengerName) =>
	TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

		var result = await _tripInitialRepository.Filter(StatusId, VehicleTypId, DriverName, PassengerName);
		if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}