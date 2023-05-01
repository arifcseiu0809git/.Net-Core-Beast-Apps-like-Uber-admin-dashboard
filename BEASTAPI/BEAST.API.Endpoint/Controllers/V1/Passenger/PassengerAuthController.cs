using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using BEASTAPI.Core.Model;
using BEASTAPI.Endpoint.Resources;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Model.Passenger;


namespace BEASTAPI.Endpoint.Controllers.V1.Passenger;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class PassengerAuthController : ControllerBase
{
    private readonly ILogger<PassengerAuthController> _logger;
    private readonly IConfiguration _config;
    private readonly IPassengerAuthRepository _passengerAuthRepository;
    private readonly ISMSSender _sMSSender;
    private readonly ISecurityHelper _securityHelper;
    private readonly ICsvExporter _csvExporter;

    public PassengerAuthController(ILogger<PassengerAuthController> logger, IConfiguration config, IPassengerAuthRepository passengerAuthRepository, ISMSSender sMSSender, ICsvExporter csvExporter, ISecurityHelper securityHelper)
    {
        this._logger = logger;
        this._config = config;
        this._passengerAuthRepository = passengerAuthRepository;
        this._sMSSender = sMSSender;
        this._securityHelper = securityHelper;
        this._csvExporter = csvExporter;
    }

    [HttpPost("Register"), AllowAnonymous]
    public Task<IActionResult> Register([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PassengerModel passengerInfo = JsonSerializer.Deserialize<PassengerModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passengerInfo.FirstName+" "+passengerInfo.LastName))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (passengerInfo == null) return BadRequest(ValidationMessages. MobileAuth_RegisterNull);

        RegisterResponseModel response = await _passengerAuthRepository.Register(passengerInfo,logModel);
        return (response == null) ? BadRequest() : Ok(response);
    });

    [HttpPost("GenPassengerAuthOTP"), AllowAnonymous]
    public Task<IActionResult> GenPassengerAuthOTP([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        PassengerModel passenger = JsonSerializer.Deserialize<PassengerModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        passenger.Id = Guid.NewGuid().ToString();
        passenger.CreatedDate = DateTime.Now;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passenger.UserId))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (passenger == null)
            return BadRequest(ValidationMessages.Auth_UserInfoNull);

        UserInfoModel userInfo = new UserInfoModel
        {
            Id = passenger.UserId,
            Name = passenger.FirstName + " " + passenger.LastName,
            UserName=passenger.Email,
            Email = passenger.Email,
            PhoneNumber = passenger.MobileNumber,
            Role=logModel.UserRole
        };

        TokenModel token = new TokenModel
        {
            JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
            Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
            RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
            RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
        };


        string generatedOTP = await Task.Run(() => _securityHelper.GenerateOTP(passenger.MobileNumber));
        if(!String.IsNullOrEmpty(generatedOTP))
        {
            passenger.OtpCode = generatedOTP;
            passenger.StartTime = DateTime.Now;
            passenger.EndTime = DateTime.Now.AddMinutes(10);
            passenger.TimeInMinutes = 10;
            await _passengerAuthRepository.InsertOTP(passenger,logModel);

            List<string> lstMobileNo = new List<string>();
            lstMobileNo.Add(passenger.MobileNumber);

            SMSModel model = new SMSModel();
            model.To = lstMobileNo;
            model.Content = generatedOTP + " is your One-Time Password, valid for 10 minutes only, Please do not share your OTP with anyone.";

            await Task.Run(() => _sMSSender.SendSMS(model));
        }

        return Ok(token);
    });

    [HttpPost("ValidatePassengerAuthOTP"), AllowAnonymous]
    public Task<IActionResult> ValidatePassengerAuthOTP([FromBody] PassengerModel passengerInfo) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passengerInfo.UserId))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (passengerInfo == null)
            return BadRequest(ValidationMessages.Auth_UserInfoNull);

        var result = await _passengerAuthRepository.ValidatePassengerAuthOTP(passengerInfo);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, passengerInfo.UserId));

        OTPModel oTPModel = new OTPModel
        {
            MobileNo=passengerInfo.MobileNumber,
            Status=true,
            Message="OTP validated successfully."
        };

        return Ok(oTPModel);
    });

    [HttpGet]
    public Task<IActionResult> GetPassengers(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber <= 0)
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

        var result = await _passengerAuthRepository.GetPassengers(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Pie_NotFoundList);

        return Ok(result);
    });
	[HttpGet("GetPassengerRideHistoriesById/{passengerId}/{pageNumber}")]
	public Task<IActionResult> GetPassengerRideHistoriesById(string passengerId, int pageNumber) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passengerId))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _passengerAuthRepository.GetPassengerRideHistories(passengerId, pageNumber);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, passengerId));

		return Ok(result);
	});
	
	[HttpGet("GetPassengerPaymentHistoriesById/{passengerId}/{pageNumber}")]
	public Task<IActionResult> GetPassengerPaymentHistoriesById(string passengerId, int pageNumber) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), passengerId))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _passengerAuthRepository.GetPassengerPaymentHistories(passengerId, pageNumber);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, passengerId));

		return Ok(result);
	});
	[HttpGet("{id}")]
    public Task<IActionResult> GetPassengerById(string id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var result = await _passengerAuthRepository.GetPassengerById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        return Ok(result);
    });

    [HttpGet("GetDistinctPassengers"), AllowAnonymous]
    public Task<IActionResult> GetDistinctPassengers() =>
   TryCatch(async () =>
   {
       #region Validation
       if (Convert.ToBoolean(_config["Hash:HashChecking"]))
       {
           if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
               return Unauthorized(ValidationMessages.InvalidHash);
       }
       #endregion

       var result = await _passengerAuthRepository.GetDistinctPassengers();
       if (result == null)
           return NotFound(ValidationMessages.Pie_NotFoundCategoryId);

       return Ok(result);
   });

    [HttpPut("Update/{id}"), AllowAnonymous]
    public Task<IActionResult> UpdatePassenger(string id, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        PassengerModel passenger = JsonSerializer.Deserialize<PassengerModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        passenger.CreatedDate = DateTime.Now;
        passenger.EnrollDate = DateTime.Now;

        LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (passenger == null)
            return BadRequest(ValidationMessages.Pie_Null);

        if (id != passenger.Id)
            return BadRequest(ValidationMessages.Pie_Mismatch);

        var passen = await _passengerAuthRepository.GetPassengerById(id);
        if (passen == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _passengerAuthRepository.UpdatePassenger(passenger, logModel);

        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeletePassenger(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (string.IsNullOrEmpty(id))
            return BadRequest(String.Format(ValidationMessages.Pie_InvalidId, id));

        var passengerToDelete = await _passengerAuthRepository.GetPassengerById(id);
        if (passengerToDelete == null)
            return NotFound(String.Format(ValidationMessages.Pie_NotFoundId, id));

        await _passengerAuthRepository.DeletePassenger(id, logModel);

        return NoContent(); // success
    });
	[HttpGet("Export/{StatusId}/{City}/{ContactNo}")]
	public Task<IActionResult> Export(string StatusId,string City,string ContactNo) =>
    TryCatch(async () =>
    {
        dynamic result = null;
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        //var result = await _passengerAuthRepository.Export(StatusId, City, ContactNo);
		result = await _passengerAuthRepository.Export(StatusId);

		if (City != "City")
		{
			if (ContactNo != "ContactNo")
			{
				result = await _passengerAuthRepository.ExportWithCity_N_ContactNo(StatusId, City, ContactNo);
			}
			else
			{
				result = await _passengerAuthRepository.ExportWithCity(StatusId, City);
			}
		}
		else
		{
			if (ContactNo != "ContactNo")
			{
				result = await _passengerAuthRepository.ExportWithContactNo(StatusId, ContactNo);
			}
		}



		if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });
	[HttpGet("ExportPassengerRideHistory/{passengerId}")]
	public Task<IActionResult> ExportPassengerRideHistory(string passengerId) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _passengerAuthRepository.ExportPassengerRideHistory(passengerId);
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});
	
	[HttpGet("ExportPassengerPaymentHistory/{passengerId}")]
	public Task<IActionResult> ExportPassengerPaymentHistory(string passengerId) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _passengerAuthRepository.ExportPassengerPaymentHistory(passengerId);
		if (result == null)
			return NotFound(ValidationMessages.Category_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost("Login"), AllowAnonymous]
    public Task<IActionResult> Login([FromBody] UserLoginModel userLogin) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userLogin.UserName))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (userLogin == null) return BadRequest(ValidationMessages.Auth_LoginNull);
        #endregion

        TokenModel token = new TokenModel();
        UserInfoModel userInfo = await _passengerAuthRepository.Login(userLogin);
        if (userInfo != null)
        {
            token.JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo));
            token.Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"]));
            token.RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken());
            token.RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]));

            await _passengerAuthRepository.UpdateRefreshToken(userInfo.Id, token);
        }

        return (userInfo == null) ? Unauthorized() : Ok(token);
    });

    [HttpPost("ChangePassword"), AllowAnonymous]
    public Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel changePassword) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), changePassword.CurrentPassword))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (changePassword == null) return BadRequest(ValidationMessages.MobileAuth_ChangePasswordNull);

        UserInfoModel userInfo = await _passengerAuthRepository.ChangePassword(changePassword);
        return (userInfo == null) ? BadRequest() : Ok(userInfo);

    });

    [HttpPost("GetToken"), AllowAnonymous]
    public Task<IActionResult> GetToken([FromBody] UserInfoModel userInfo) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userInfo.Id))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (userInfo == null)
            return BadRequest(ValidationMessages.Auth_UserInfoNull);

        TokenModel token = new TokenModel
        {
            JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
            Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
            RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
            RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
        };

        await _passengerAuthRepository.UpdateRefreshToken(userInfo.Id, token);

        return Ok(token);
    });

    [HttpPost("RefreshToken"), AllowAnonymous]
    public Task<IActionResult> RefreshToken(string payLoad = null) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var identity = HttpContext.User.Identity as ClaimsIdentity;
        if (identity == null) return Unauthorized();

        string userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        UserInfoModel userInfo = await _passengerAuthRepository.GetCurrentUser(userId);
        if (userInfo == null) return Unauthorized();

        TokenModel refreshToken = await _passengerAuthRepository.GetRefreshToken(userId);
        if (!refreshToken.RefreshToken.Equals(Request.Cookies["X-RefreshToken"].ToString()))
            return Unauthorized(ValidationMessages.Auth_InvalidRefreshToken);
        else if (refreshToken.RefreshTokenExpires < DateTime.Now)
            return Unauthorized(ValidationMessages.Auth_ExpiredRefreshToken);

        TokenModel token = new TokenModel
        {
            JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
            Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
            RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
            RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
        };

        await _passengerAuthRepository.UpdateRefreshToken(userInfo.Id, token);

        return Ok(token);
    });

    [HttpGet("GetCurrentUser"), AllowAnonymous]
    public IActionResult GetCurrentUser() =>
    TryCatch(() =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        var identity = HttpContext.User.Identity as ClaimsIdentity;

        if (identity != null)
        {
            var claims = identity.Claims;
            var userInfo = new UserInfoModel
            {
                Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            };

            return Ok(userInfo);
        }

        return null;
    });
	[HttpGet("Filter/{StatusId}/{City}/{ContactNo}")]
	public Task<IActionResult> Filter(string StatusId, string City, string ContactNo) =>
	TryCatch(async () =>
	{
        dynamic result = null;
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), StatusId + City + ContactNo.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (string.IsNullOrEmpty(StatusId) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(ContactNo))
			return BadRequest(String.Format(ValidationMessages.Invalid_Filtering, "Select Valid Filtering"));

		result = await _passengerAuthRepository.Filter(StatusId);

		if (City != "City")
        {
            if(ContactNo != "ContactNo")
            {
				result = await _passengerAuthRepository.FilterWithCity_N_ContactNo(StatusId, City, ContactNo);
			}
            else
            {
				result = await _passengerAuthRepository.FilterWithCity(StatusId, City);
			}
        }
        else
        {
            if(ContactNo != "ContactNo")
            {
				result = await _passengerAuthRepository.FilterWithContactNo(StatusId, ContactNo);
			}
        }

		
		if (result == null)
			return NotFound(String.Format(ValidationMessages.Pie_NotFoundList));

		return Ok(result);
	});
}