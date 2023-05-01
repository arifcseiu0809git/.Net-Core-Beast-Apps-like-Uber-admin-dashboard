using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Contract;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;
using BEASTAPI.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Endpoint.Resources;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1.Passenger;


[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PassengersPaymentMethodController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<PassengersPaymentMethodController> _logger;
    private readonly IConfiguration _config;
    private readonly IPassengersPaymentMethodRepository _passengerPaymentMethodRepository;
    private readonly ICsvExporter _csvExporter;

    public PassengersPaymentMethodController(ISecurityHelper securityHelper, ILogger<PassengersPaymentMethodController> logger, IConfiguration config, IPassengersPaymentMethodRepository passengerPaymentMethodRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._passengerPaymentMethodRepository = passengerPaymentMethodRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetPassengersPaymentMethods(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _passengerPaymentMethodRepository.GetPassengersPaymentMethods(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetPassengersPaymentMethodById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _passengerPaymentMethodRepository.GetPassengersPaymentMethodById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpPost]
    public Task<IActionResult> InsertPassengersPaymentMethod([FromBody] Dictionary<string, object> PostData) =>
     TryCatch(async () =>
     {
         PassengersPaymentMethodModel passengerPaymentMethod = JsonSerializer.Deserialize<PassengersPaymentMethodModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
         LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

         if (Convert.ToBoolean(_config["Hash:HashChecking"]))
         {
             if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passengerPaymentMethod.PaymentMethodName))
                 return Unauthorized(ValidationMessages.InvalidHash);
         }

         if (passengerPaymentMethod == null)
             return BadRequest(ValidationMessages.Pie_Null);

         var existingPassengerPaymentMethod = await _passengerPaymentMethodRepository.GetPassengersPaymentMethodByName(passengerPaymentMethod.PaymentMethodName);
         if (existingPassengerPaymentMethod != null)
         {
             ModelState.AddModelError("Duplicate Passenger Payment Method", String.Format(ValidationMessages.Pie_Duplicate, passengerPaymentMethod.PaymentMethodName));
             return BadRequest(ModelState);
         }

         string insertedPassengerPaymentMethodId = await _passengerPaymentMethodRepository.InsertPassengersPaymentMethod(passengerPaymentMethod, logModel);
         return Created(nameof(GetPassengersPaymentMethodById), new { id = insertedPassengerPaymentMethodId });
     });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdatePassengersPaymentMethod(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        PassengersPaymentMethodModel passengerPaymentMethod = JsonSerializer.Deserialize<PassengersPaymentMethodModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (passengerPaymentMethod == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != passengerPaymentMethod.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var passengerLocationToUpdate = await _passengerPaymentMethodRepository.GetPassengersPaymentMethodById(id);
        if (passengerLocationToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _passengerPaymentMethodRepository.UpdatePassengersPaymentMethod(passengerPaymentMethod, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePassengersPaymentMethod(string id, [FromBody] LogModel logModel) =>
      TryCatch(async () =>
      {
          if (Convert.ToBoolean(_config["Hash:HashChecking"]))
          {
              if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                  return Unauthorized(ValidationMessages.InvalidHash);
          }

          if (string.IsNullOrEmpty(id))
              return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

          var passengerPaymentMethodToDelete = await _passengerPaymentMethodRepository.GetPassengersPaymentMethodById(id);
          if (passengerPaymentMethodToDelete == null)
              return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

          await _passengerPaymentMethodRepository.DeletePassengersPaymentMethod(id, logModel);

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

        var result = await _passengerPaymentMethodRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
}