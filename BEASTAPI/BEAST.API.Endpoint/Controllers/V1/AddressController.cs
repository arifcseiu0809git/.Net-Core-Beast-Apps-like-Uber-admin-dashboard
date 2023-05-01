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
using BEASTAPI.Core.Contract.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    public partial class AddressController : ControllerBase
    {
        private readonly ISecurityHelper _securityHelper;
        private readonly ILogger<PieController> _logger;
        private readonly IConfiguration _config;
        private readonly IAddressRepository _addressRepository; 
        private readonly ICsvExporter _csvExporter;


        public AddressController(ISecurityHelper securityHelper, ILogger<PieController> logger, IConfiguration config, IAddressRepository addressRepository, ICsvExporter csvExporter)
        {
            this._securityHelper = securityHelper;
            this._logger = logger;
            this._config = config;
            this._addressRepository = addressRepository;             
            this._csvExporter = csvExporter;
        }

        [HttpGet]
        public Task<IActionResult> GetAddress(int pageNumber) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (pageNumber <= 0)
                return BadRequest(String.Format(ValidationMessages.Address_InvalidPageNumber, pageNumber));

            var result = await _addressRepository.GetAddress(pageNumber);
            if (result == null)
                return NotFound(ValidationMessages.Address_NotFoundList);

            return Ok(result);
        });

        [HttpGet("{AddressId}")]
        public Task<IActionResult> GetAddressById(string AddressId) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), AddressId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (AddressId == null)
            return BadRequest(String.Format(ValidationMessages.Address_InvalidId, AddressId));

        var result = await _addressRepository.GetAddressById(AddressId);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Address_NotFoundId, AddressId));

        return Ok(result);
    });

        [HttpPost]
        public Task<IActionResult> InsertAddress([FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            AddressModel address = JsonSerializer.Deserialize<AddressModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            address.Id = Guid.NewGuid().ToString();
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), address.UserId))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (address == null)
                return BadRequest(ValidationMessages.Address_Null);
 
            string insertedAddressId = await _addressRepository.InsertAddress(address, logModel);
            return Created(nameof(GetAddressById), new { id = insertedAddressId });
        });

        //, Authorize(Roles = "SystemAdmin")

        [HttpPut("Update/{id}")]
        public Task<IActionResult> UpdateAddress(string id, [FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            AddressModel address = JsonSerializer.Deserialize<AddressModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (address == null)
                return BadRequest(ValidationMessages.Address_Null);

            if (id != address.Id)
                return BadRequest(ValidationMessages.Address_Mismatch);

            var addressToUpdate = await _addressRepository.GetAddressById(id);
            if (addressToUpdate == null)
                return NotFound(String.Format(ValidationMessages.Address_NotFoundId, id));

            await _addressRepository.UpdateAddress(address, logModel);

            return NoContent(); // success
        });

        [HttpPut("Delete/{id}")]
        public Task<IActionResult> DeleteAddress(string id, [FromBody] LogModel logModel) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (id == null)
                return BadRequest(String.Format(ValidationMessages.Address_InvalidId, id));

            var addressToDelete = await _addressRepository.GetAddressById(id);
            if (addressToDelete == null)
                return NotFound(String.Format(ValidationMessages.Address_NotFoundId, id));

            await _addressRepository.DeleteAddress(id, logModel);

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

            var result = await _addressRepository.Export();
            if (result == null)
                return NotFound(ValidationMessages.Address_NotFoundList);

            return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
        });

    }
}
