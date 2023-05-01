using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Endpoint.Controllers.V1.Common;
using BEASTAPI.Endpoint.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace BEASTAPI.Endpoint.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    public partial class XDriverVehicleController : ControllerBase
    {
        private readonly ISecurityHelper _securityHelper;
        private readonly ILogger<XDriverVehicleController> _logger;
        private readonly IConfiguration _config;
        private readonly IXDriverVehicleRepository _xDriverVehicleModelRepository;
        private readonly ICsvExporter _csvExporter;

        public XDriverVehicleController(ISecurityHelper securityHelper, ILogger<XDriverVehicleController> logger, IConfiguration config, IXDriverVehicleRepository xDriverVehicleModelRepository, ICsvExporter csvExporter)
        {
            this._securityHelper = securityHelper;
            this._logger = logger;
            this._config = config;
            this._xDriverVehicleModelRepository = xDriverVehicleModelRepository;
            this._csvExporter = csvExporter;
        }

        [HttpGet]
        public Task<IActionResult> Get(int pageNumber) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (pageNumber <= 0)
                return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

            var result = await _xDriverVehicleModelRepository.GetXDriverVehicleModels(pageNumber);
            if (result == null)
                return NotFound(ValidationMessages.Pie_NotFoundList);

            return Ok(result);
        });

        [HttpGet("{id}")]
        public Task<IActionResult> GetById(string id) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }
            var result = await _xDriverVehicleModelRepository.GetXDriverVehicleModelById(id);
            if (result == null)
                return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

            return Ok(result);
        });

        [HttpPost]
        public Task<IActionResult> Insert([FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            XDriverVehicleModel xDriverVehicleModel = JsonSerializer.Deserialize<XDriverVehicleModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            xDriverVehicleModel.Id = Guid.NewGuid().ToString();

            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), xDriverVehicleModel.VehicleId.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (xDriverVehicleModel == null)
                return BadRequest(ValidationMessages.Pie_Null);

            string insertedId = await _xDriverVehicleModelRepository.InsertXDriverVehicleModel(xDriverVehicleModel, logModel);
            return Created(nameof(Get), new { id = insertedId });
        });

        [HttpPut("Update/{id}")]
        public Task<IActionResult> Update(string id, [FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            XDriverVehicleModel xDriverVehicleModel = JsonSerializer.Deserialize<XDriverVehicleModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (xDriverVehicleModel == null)
                return BadRequest(ValidationMessages.Pie_Null);

            if (id != xDriverVehicleModel.Id)
                return BadRequest(ValidationMessages.Pie_Mismatch);

            var xDriverVehicleModelToUpdate = await _xDriverVehicleModelRepository.GetXDriverVehicleModelById(id);
            if (xDriverVehicleModelToUpdate == null)
                return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

            await _xDriverVehicleModelRepository.UpdateXDriverVehicleModel(xDriverVehicleModel, logModel);

            return NoContent(); // success
        });

        [HttpPut("Delete/{id}")]
        public Task<IActionResult> Delete(string id, [FromBody] LogModel logModel) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (id == "")
                return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

            var xDriverVehicleModelToDelete = await _xDriverVehicleModelRepository.GetXDriverVehicleModelById(id);
            if (xDriverVehicleModelToDelete == null)
                return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

            await _xDriverVehicleModelRepository.DeleteXDriverVehicleModel(id, logModel);

            return NoContent(); // success
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

            var result = await _xDriverVehicleModelRepository.Export();
            if (result == null)
                return NotFound(ValidationMessages.Category_NotFoundList);

            return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
        });

		[HttpGet("Drivers/{prefix}")]
		public Task<IActionResult> GetDriversBySearchPrefix(string prefix) =>
	    TryCatch(async () =>
	    {
		    if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		    {
			    if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), prefix.ToString()))
				    return Unauthorized(ValidationMessages.InvalidHash);
		    }

		    if (string.IsNullOrEmpty(prefix))
			    return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, prefix));

		    var result = await _xDriverVehicleModelRepository.GetDriversBySearchPrefix(prefix);
		    if (result == null)
			    return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, prefix));

		    return Ok(result);
	    });

		[HttpGet("Vehicles/{prefix}")]
		public Task<IActionResult> GetVehiclesBySearchPrefix(string prefix) =>
		TryCatch(async () =>
		{
			if (Convert.ToBoolean(_config["Hash:HashChecking"]))
			{
				if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), prefix.ToString()))
					return Unauthorized(ValidationMessages.InvalidHash);
			}

			if (string.IsNullOrEmpty(prefix))
				return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, prefix));

			var result = await _xDriverVehicleModelRepository.GetVehiclesBySearchPrefix(prefix);
			if (result == null)
				return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, prefix));

			return Ok(result);
		});
	}
}
