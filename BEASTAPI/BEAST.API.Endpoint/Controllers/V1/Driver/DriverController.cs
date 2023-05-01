using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using BEASTAPI.Core.Model.Driver;
using BEASTAPI.Endpoint.Resources;
using BEASTAPI.Core.Contract.Infrastructure;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Driver;

namespace BEASTAPI.Endpoint.Controllers.V1.Driver;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class DriverController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<DriverController> _logger;
    private readonly IConfiguration _config;
    private readonly IDriverRepository _driverRepository;
    private readonly ICsvExporter _csvExporter;

    public DriverController(ISecurityHelper securityHelper, ILogger<DriverController> logger, IConfiguration config, IDriverRepository driverRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._driverRepository = driverRepository;
        this._csvExporter = csvExporter;
    }


    [HttpPost("Register"), AllowAnonymous]
    public Task<IActionResult> Register([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        DriverModel driverModel = JsonSerializer.Deserialize<DriverModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        driverModel.Id = Guid.NewGuid().ToString();


        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), driverModel.FirstName + " " + driverModel.LastName))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (driverModel == null) return BadRequest(ValidationMessages.MobileAuth_RegisterNull);

        RegisterResponseModel response = await _driverRepository.Register(driverModel, logModel);
        return (response == null) ? BadRequest() : Ok(response);
    });

    [HttpPost("SendOtp"), AllowAnonymous]
    public Task<IActionResult> SendOtp(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        DriverModel driverModel = JsonSerializer.Deserialize<DriverModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (driverModel == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != driverModel.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        UserInfoModel userInfo = new UserInfoModel
        {
            Id = driverModel.UserId,
            Name = driverModel.FirstName + " " + driverModel.MiddleName + " " + driverModel.LastName,
            UserName = driverModel.Email,
            Email = driverModel.Email,
            PhoneNumber = driverModel.MobileNumber,
            Role = logModel.UserRole
        };

        //string generatedOTPMethod = !string.IsNullOrEmpty(driverModel.Email) ? driverModel.Email : driverModel.MobileNumber;
        string generatedOTP = await Task.Run(() => _securityHelper.GenerateOTP(driverModel.MobileNumber));
        RegisterResponseModel response = await _driverRepository.SendOTP(id, driverModel, generatedOTP);
        TokenModel token = new TokenModel
        {
            JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
            Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
            RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
            RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
        };
        return (response == null) ? BadRequest() : Ok(response);
    });


    [HttpPost("ValidateDriverOTP"), AllowAnonymous]
    public Task<IActionResult> ValidateDriverOTP(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        DriverModel driverModel = JsonSerializer.Deserialize<DriverModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (driverModel == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != driverModel.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var driverToUpdate = await _driverRepository.GetDriverById(id);

        if (driverModel == null)
            return BadRequest(ValidationMessages.Auth_UserInfoNull);

        var result = await _driverRepository.ValidateDriverOTP(driverModel);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, driverModel.UserId));

        OTPModel oTPModel = new OTPModel
        {
            MobileNo = driverModel.MobileNumber,
            Status = true,
            Message = "OTP validated successfully."
        };

        return Ok(oTPModel);
    });

    [HttpGet]
    public Task<IActionResult> GetDrivers(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _driverRepository.GetDrivers(pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });
    [HttpGet("GetDriverStatusWise/{pageNumber}/{IsApproved}/{StatusId}/{NID}/{DrivingLicenseNo}")]
    public Task<IActionResult> GetDrivers(int pageNumber, bool IsApproved,string StatusId, string NID, string DrivingLicenseNo) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), StatusId + NID + DrivingLicenseNo+pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _driverRepository.GetDrivers(pageNumber, IsApproved, StatusId, NID, DrivingLicenseNo);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });



    [HttpGet("{id}")]
    public Task<IActionResult> GetDriverById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var result = await _driverRepository.GetDriverById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpGet("GetDistinctDrivers"), AllowAnonymous]
    public Task<IActionResult> GetDistinctDrivers() =>
   TryCatch(async () =>
   {
       #region Validation
       if (Convert.ToBoolean(_config["Hash:HashChecking"]))
       {
           if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
               return Unauthorized(ValidationMessages.InvalidHash);
       }
       #endregion

       var result = await _driverRepository.GetDistinctDrivers();
       if (result == null)
           return NotFound(ValidationMessages.Pie_NotFoundCategoryId);

       return Ok(result);
   }); [HttpGet("GetActiveDrivers"), AllowAnonymous]
    public Task<IActionResult> GetActiveDrivers() =>
   TryCatch(async () =>
   {
       #region Validation
       if (Convert.ToBoolean(_config["Hash:HashChecking"]))
       {
           if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
               return Unauthorized(ValidationMessages.InvalidHash);
       }
       #endregion

       var result = await _driverRepository.GetActiveDrivers();
       if (result == null)
           return NotFound(ValidationMessages.Pie_NotFoundCategoryId);

       return Ok(result);
   });

    [HttpPost]
    public Task<IActionResult> InsertDriver([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {

        DriverModel driverModel = JsonSerializer.Deserialize<DriverModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        driverModel.Id = Guid.NewGuid().ToString();

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), driverModel.DrivingLicenseNo))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (driverModel == null)
            return BadRequest(String.Format(ValidationMessages.Pie_Null));

        string insertedDriverId = await _driverRepository.InsertDriver(driverModel, logModel);
        return Created(nameof(GetDriverById), new { id = insertedDriverId });
    });

    [HttpPut("Update/{id}")]
    public Task<IActionResult> UpdateDriver(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        DriverModel driver = JsonSerializer.Deserialize<DriverModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (driver == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != driver.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var driverToUpdate = await _driverRepository.GetDriverById(id);
        if (driverToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _driverRepository.UpdateDriver(driver, logModel);
        return NoContent();
    });
    [HttpPut("UpdateStatus/{id}")]
    public Task<IActionResult> UpdateStatus(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        DriverModel driver = JsonSerializer.Deserialize<DriverModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (driver == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != driver.Id)
            return BadRequest(String.Format(ValidationMessages.Pie_Mismatch));

        var driverToUpdate = await _driverRepository.GetDriverById(id);
        if (driverToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _driverRepository.UpdateStatus(driver, logModel);
        return NoContent();
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteDriver(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var DriverToDelete = await _driverRepository.GetDriverById(id);
        if (DriverToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _driverRepository.DeleteDriver(id, logModel);
        return NoContent();
    });

	[HttpGet("Filter/{IsApproved}/{StatusId}/{NID}/{DrivingLicenseNo}")]
	public Task<IActionResult> Filter(bool IsApproved, string StatusId, string NID, string DrivingLicenseNo) =>
TryCatch(async () =>
{
	if (Convert.ToBoolean(_config["Hash:HashChecking"]))
	{
		if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), StatusId + NID + DrivingLicenseNo.ToString()))
			return Unauthorized(ValidationMessages.InvalidHash);
	}

	if (string.IsNullOrEmpty(StatusId) && string.IsNullOrEmpty(NID) && string.IsNullOrEmpty(DrivingLicenseNo))
		return BadRequest(String.Format(ValidationMessages.Invalid_Filtering, "Select Valid Filtering"));

	var result = await _driverRepository.Filter(IsApproved, StatusId, NID, DrivingLicenseNo);
	if (result == null)
		return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

	return Ok(result);
});


	[HttpGet("Export/{StatusId}/{NID}/{DrivingLicenseNo}/{IsApproved}")]
	// public Task<IActionResult> Export() =>
	public Task<IActionResult> Export(string StatusId, string NID, string DrivingLicenseNo, bool IsApproved) =>
   TryCatch(async () =>
   {
	   if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _driverRepository.Export(StatusId, NID, DrivingLicenseNo, IsApproved);
		if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpGet("{driverId}/GetDriverCommission/{pageNumber}/")]
    public Task<IActionResult> GetDriverCommission(string driverId, int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _driverRepository.GetDriverCommissions(driverId, pageNumber);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

        return Ok(result);
    });

}