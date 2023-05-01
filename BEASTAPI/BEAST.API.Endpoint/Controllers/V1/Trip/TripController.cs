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
using BEASTAPI.Core.Contract.Persistence.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1.Common;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class TripController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<TripController> _logger;
    private readonly IConfiguration _config;
    private readonly ITripRepository _tripRepository;
    private readonly ICsvExporter _csvExporter;

    public TripController(ISecurityHelper securityHelper, ILogger<TripController> logger, IConfiguration config, ITripRepository tripRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._tripRepository = tripRepository;
        this._csvExporter = csvExporter;
    }

	[HttpGet("GetTrips/{statusId}/{pageNumber}")]
	public Task<IActionResult> GetTrips(string statusId, int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
       
        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _tripRepository.GetTrips(statusId, pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetTripById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        var result = await _tripRepository.GetTripById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

	[HttpGet("GetTripsByDriverId/{id}/{pageNumber}")]
	public Task<IActionResult> GetTripsByDriverId(string id,int pageNumber) =>
TryCatch(async () =>
{
	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	{
		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
			return Unauthorized(ValidationMessages.InvalidHash);
	}
	var result = await _tripRepository.GetTripsByDriverId(id, pageNumber);
	if (result == null)
		return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

	return Ok(result);
});


	[HttpPost]
    public Task<IActionResult> InsertTrip([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        TripModel trip = JsonSerializer.Deserialize<TripModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        trip.Id = new Guid().ToString();

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), trip.VehicleId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (trip == null)
            return BadRequest(ValidationMessages.Pie_Null);

        string insertedTripId = await _tripRepository.InsertTrip(trip, logModel);
        return Created(nameof(GetTripById), new { id = insertedTripId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateTrip(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        TripModel trip = JsonSerializer.Deserialize<TripModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (trip == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != trip.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var tripToUpdate = await _tripRepository.GetTripById(id);
        if (tripToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _tripRepository.UpdateTrip(trip, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteTrip(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (id == "")
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var tripToDelete = await _tripRepository.GetTripById(id);
        if (tripToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _tripRepository.DeleteTrip(id, logModel);

        return NoContent(); // success
    });






    [HttpGet("Filter/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}/{ContactNo}")]
    public Task<IActionResult> Filter(string StatusId, string VehicleTypId, string DriverName, string PassengerName, string ContactNo) =>
TryCatch(async () =>
{
    if (Convert.ToBoolean(_config["Hash:HashChecking"]))
    {
        if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), StatusId + VehicleTypId + DriverName + PassengerName.ToString()))
            return Unauthorized(ValidationMessages.InvalidHash);
    }

    if (string.IsNullOrEmpty(StatusId) && string.IsNullOrEmpty(VehicleTypId) && string.IsNullOrEmpty(DriverName) && string.IsNullOrEmpty(PassengerName))
        return BadRequest(String.Format(ValidationMessages.Invalid_Filtering, "Select Valid Filtering"));

    var result = await _tripRepository.Filter(StatusId, VehicleTypId, DriverName, PassengerName, ContactNo);
    if (result == null)
        return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

    return Ok(result);
});


    [HttpGet("Export/{StatusId}/{VehicleTypId}/{DriverName}/{PassengerName}/{ContactNo}")]
    public Task<IActionResult> Export(string StatusId, string VehicleTypId, string DriverName, string PassengerName, string ContactNo) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _tripRepository.Filter(StatusId, VehicleTypId, DriverName, PassengerName, ContactNo);
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

}