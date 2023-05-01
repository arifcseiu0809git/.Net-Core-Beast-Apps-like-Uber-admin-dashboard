using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BEASTAPI.Persistence.Identity;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using System.Data;
using BEASTAPI.Core.Model;
using System.Text;
using System.Text.Encodings.Web;
using BEASTAPI.Core.Model.Passenger;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Infrastructure;



namespace BEASTAPI.Persistence.Passenger;

public class PassengerAuthRepository : IPassengerAuthRepository
{
    private readonly ILogger<PassengerAuthRepository> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpContext _context;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private const string PassengerAuthCache = "PassengerAuthData";
    private const string DistinctPassengerAuthCache = "DistinctPassengerAuthData";
	private const string TripCache = "TripData";

	public PassengerAuthRepository(ILogger<PassengerAuthRepository> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor, IAuditLogRepository auditLogRepository, IEmailSender emailSender, IEmailTemplateRepository emailTemplateRepository, IMemoryCache cache, IConfiguration config, IDataAccessHelper dataAccessHelper)
    {
        this._logger = logger;
        this._signInManager = signInManager;
        this._userManager = userManager;
        this._context = accessor.HttpContext;
        this._auditLogRepository = auditLogRepository;
        this._emailSender = emailSender;
        this._emailTemplateRepository = emailTemplateRepository;
        this._dataAccessHelper = dataAccessHelper;
        this._cache = cache;
        this._config = config;
        //this._passengerRepository = passengerRepository;
    }

    #region "DataAccessHelper Methods"
    public async Task<RegisterResponseModel> Register(PassengerModel passengerInfo, LogModel logModel)
    {
        ApplicationUser applicationUser = new ApplicationUser
        {
            FullName = passengerInfo.FirstName + " " + passengerInfo.LastName,
            UserName = passengerInfo.Email,
            EmailConfirmed = true,
            UserType=passengerInfo.UserType,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            Email = passengerInfo.Email,
            PhoneNumber = passengerInfo.MobileNumber,
            LockoutEnabled = false,
            AccessFailedCount = 0
        };

        //var result = await _userManager.CreateAsync(applicationUser, userInfo.Password);
        try
        {
            var result = await _userManager.CreateAsync(applicationUser);


            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(applicationUser, logModel.UserRole);
                await _userManager.AddClaimAsync(applicationUser, new System.Security.Claims.Claim("FullName", passengerInfo.FirstName + " " + passengerInfo.LastName));

                #region Applicaion Log
                _logger.LogInformation("User created a new account with password.");
                #endregion

                #region Audit Log
                var log = new LogModel
                {
                    UserName = _context.User.Identity.Name == null ? applicationUser.UserName : _context.User.Identity.Name,
                    UserRole = _context.User.Identity.Name == null ? "SystemAdmin" : _context.User.Claims.First(c => c.Type.Contains("role")).Value,
                    IP = Utility.GetIPAddress(_context.Request),
                    TableName = "AspNetUsers",
                    OldData = _context.User.Identity.Name == null ? null : $"<deleted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Register\" />",
                    NewData = $"<inserted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Register\" />"
                };
                _ = Task.Run(async () => { await _auditLogRepository.InsertAuditLog(log); });
                #endregion

                passengerInfo.UserId = applicationUser.Id;
                passengerInfo.CreatedDate= DateTime.Now;
                passengerInfo.EnrollDate= DateTime.Now;
                passengerInfo.Id = Guid.NewGuid().ToString();
                string insertedPassengerId = await InsertPassenger(passengerInfo, log);

                #region Send Email
                //TODO: Make URL come from settings, also check if email confirmation works
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = $@"http://localhost:5122/Identity/Account/ConfirmEmail?userId={applicationUser.Id}&code={code}";
                callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

                var emailTemplate = await _emailTemplateRepository.GetEmailTemplateByName("Confirm Email");
                emailTemplate.Template = emailTemplate.Template.Replace("$fullName", passengerInfo.FirstName + " " + passengerInfo.LastName);
                emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

                _ = Task.Run(async () =>
                {
                    await _emailSender.SendEmail(new EmailModel { To = passengerInfo.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                });
                #endregion


                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return new RegisterResponseModel { RequireConfirmedAccount = true };
                }
                else
                {
                    await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                    return new RegisterResponseModel { SignedIn = true };
                }
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<int> InsertOTP(PassengerModel passenger, LogModel logModel)
    {
        try
        {
            ClearCache(PassengerAuthCache);

            ApplicationUser applicationUser = new ApplicationUser
            {
                FullName = passenger.FirstName + " " + passenger.LastName,
                UserName= passenger.Email,
                Email = passenger.Email,
                PhoneNumber = passenger.MobileNumber
            };
            #region Audit Log
            var log = new LogModel
            {
                UserName = _context.User.Identity.Name == null ? applicationUser.UserName : _context.User.Identity.Name,
                UserRole = _context.User.Identity.Name == null ? "SystemAdmin" : _context.User.Claims.First(c => c.Type.Contains("role")).Value,
                IP = Utility.GetIPAddress(_context.Request),
                TableName = "OTP",
                OldData = _context.User.Identity.Name == null ? null : $"<deleted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Gen Passenger OTP\" />",
                NewData = $"<inserted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Gen Passenger OTP\" />"
            };
            _ = Task.Run(async () => { await _auditLogRepository.InsertAuditLog(log); });
            #endregion


            DynamicParameters p = new DynamicParameters();
            p.Add("Id",DbType.Int32, direction: ParameterDirection.Output);
            p.Add("UserId", passenger.UserId);
            p.Add("OtpCode", passenger.OtpCode);
            p.Add("StartTime", passenger.StartTime);
            p.Add("EndTime", passenger.EndTime);
            p.Add("TimeInMinutes", passenger.TimeInMinutes);
            
            p.Add("IsActive", passenger.IsActive);
            p.Add("IsDeleted", passenger.IsDeleted);
            p.Add("CreatedBy", passenger.CreatedBy);
            p.Add("CreatedDate", passenger.CreatedDate);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            //p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_OTP_Insert", p);
            return 0;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<PassengerModel> ValidatePassengerAuthOTP(PassengerModel passenger)
    {
        return (await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_OTP_Validate", new { UserId =passenger.UserId, OtpCode = passenger.OtpCode })).SingleOrDefault();
    }
    
    public async Task<PaginatedListModel<PassengerModel>> GetPassengers(int pageNumber)
    {
        PaginatedListModel<PassengerModel> output = _cache.Get<PaginatedListModel<PassengerModel>>(PassengerAuthCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));
            output = new PaginatedListModel<PassengerModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };
            _cache.Set(PassengerAuthCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            List<string> keys = _cache.Get<List<string>>(PassengerAuthCache);
            if (keys is null)
                keys = new List<string> { PassengerAuthCache + pageNumber };
            else
                keys.Add(PassengerAuthCache + pageNumber);
            _cache.Set(PassengerAuthCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }
        return output;
    }
    public async Task<PassengerModel> GetPassengerById(string passengerId)
    {
        return (await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_GetById", new { Id = passengerId })).SingleOrDefault();
    }
	public async Task<PaginatedListModel<PassengerRideHistoriesModel>> GetPassengerRideHistories(string passengerId, int pageNumber)
	{
        DynamicParameters p = new DynamicParameters();
        p.Add("passengerId", passengerId);
        p.Add("PageNumber", pageNumber);
        p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
        p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

		var result = await _dataAccessHelper.QueryData<PassengerRideHistoriesModel, dynamic>("USP_Passenger_RideHistoriesByPassengerId",  p );
		int TotalRecords = p.Get<int>("TotalRecords");
		int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));
		var output2 = new PaginatedListModel<PassengerRideHistoriesModel>
		{
			PageIndex = pageNumber,
			TotalRecords = TotalRecords,
			TotalPages = totalPages,
			HasPreviousPage = pageNumber > 1,
			HasNextPage = pageNumber < totalPages,
            Items = result.ToList()
		};
            
        return output2;
	}
	public async Task<PaginatedListModel<PassengerPaymentHistoriesModel>> GetPassengerPaymentHistories(string passengerId, int pageNumber)
	{	
		DynamicParameters p = new DynamicParameters();
		p.Add("passengerId", passengerId);
		p.Add("PageNumber", pageNumber);
		p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
		p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

		var result = await _dataAccessHelper.QueryData<PassengerPaymentHistoriesModel, dynamic>("USP_Passenger_PaymentHistoriesByPassengerId", p);
		int TotalRecords = p.Get<int>("TotalRecords");
		int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));
		var output2 = new PaginatedListModel<PassengerPaymentHistoriesModel>
		{
			PageIndex = pageNumber,
			TotalRecords = TotalRecords,
			TotalPages = totalPages,
			HasPreviousPage = pageNumber > 1,
			HasNextPage = pageNumber < totalPages,
			Items = result.ToList()
		};

		return output2;
	}

	public async Task<PassengerModel> GetPassengerByName(string passengerName)
    {
        return (await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_GetByName", new { Name = passengerName })).SingleOrDefault();
    }

    public async Task<List<PassengerModel>> GetDistinctPassengers()
    {
        var output = _cache.Get<List<PassengerModel>>(DistinctPassengerAuthCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_GetDistinct", new { });
            _cache.Set(DistinctPassengerAuthCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
    public async Task<string> InsertPassenger(PassengerModel passenger, LogModel logModel)
    {
        try
        {
            ClearCache(PassengerAuthCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", passenger.Id);
            p.Add("UserId", passenger.UserId);
            p.Add("FirstName", passenger.FirstName);
            p.Add("LastName", passenger.LastName);
            p.Add("Email", passenger.Email);
            p.Add("DateOfBirth", passenger.DateOfBirth);
            p.Add("Gender", passenger.Gender);
            p.Add("MobileNumber", passenger.MobileNumber);
            p.Add("EnrollDate", passenger.EnrollDate);
            p.Add("NID", passenger.NID);
            p.Add("SystemStatusId", passenger.SystemStatusId);
            p.Add("IsActive", passenger.IsActive);
            p.Add("IsDeleted", passenger.IsDeleted);
            p.Add("CreatedDate", passenger.CreatedDate);
            p.Add("CreatedBy", passenger.CreatedBy);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_Passenger_Insert", p);
            return passenger.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task UpdatePassenger(PassengerModel passenger, LogModel logModel)
    {
        try
        {
            if (!string.IsNullOrEmpty(passenger.Password))
            {
                var user = await _userManager.FindByIdAsync(passenger.UserId);
            if (user == null) throw new Exception("please check your old password");

            var newPassword = passenger.Password;
            var result = await _userManager.RemovePasswordAsync(user);
            if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors));
            var result2 = await _userManager.AddPasswordAsync(user, newPassword);
            if (!result2.Succeeded) throw new Exception(string.Join(", ", result2.Errors));
            //return DataHelpers.ReturnJsonData(null, true, "Password changed successful");               
            }
                
            ClearCache(PassengerAuthCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", passenger.Id);
            //p.Add("UserId", passenger.UserId);            
            p.Add("FirstName", passenger.FirstName);
            p.Add("LastName", passenger.LastName);
            p.Add("Email", passenger.Email);
            p.Add("DateOfBirth", passenger.DateOfBirth);
            p.Add("Gender", passenger.Gender);
            p.Add("MobileNumber", passenger.MobileNumber);
            p.Add("EnrollDate", passenger.EnrollDate);
            p.Add("NID", passenger.NID);
            p.Add("SystemStatusId", passenger.SystemStatusId);
            p.Add("IsActive", passenger.IsActive);
            p.Add("IsDeleted", passenger.IsDeleted);
            p.Add("ModifiedBy", passenger.ModifiedBy);
            //p.Add("ModifiedDate", passenger.ModifiedDate);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_Passenger_Update", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task DeletePassenger(string passengerId, LogModel logModel)
    {
        try
        {
            ClearCache(PassengerAuthCache);
            DynamicParameters p = new DynamicParameters();
            p.Add("Id", passengerId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_Passenger_Delete", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
	public async Task<List<PassengerExportModel>> Export(string StatusId)
    {
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		return await _dataAccessHelper.QueryData<PassengerExportModel, dynamic>("USP_Passenger_Export", p);
    }
	public async Task<List<PassengerExportModel>> ExportWithContactNo(string StatusId, string ContactNo)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("ContactNo", ContactNo);
		return await _dataAccessHelper.QueryData<PassengerExportModel, dynamic>("USP_Passenger_ExportWithContactNo", p);
	}
	public async Task<List<PassengerExportModel>> ExportWithCity_N_ContactNo(string StatusId, string City, string ContactNo)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("City", City);
		p.Add("ContactNo", ContactNo);
		return await _dataAccessHelper.QueryData<PassengerExportModel, dynamic>("USP_Passenger_ExportWithCity_N_ContactNo", p);
	}
	public async Task<List<PassengerExportModel>> ExportWithCity(string StatusId, string City)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("City", City);
		return await _dataAccessHelper.QueryData<PassengerExportModel, dynamic>("USP_Passenger_ExportWithCity", p);
	}
	public async Task<List<PassengerRideHistoriesModel>> ExportPassengerRideHistory(string passengerId)
	{
		return await _dataAccessHelper.QueryData<PassengerRideHistoriesModel, dynamic>("USP_Passenger_Ride_History_File", new { passengerId = passengerId });
	}
	public async Task<List<PassengerPaymentHistoriesModel>> ExportPassengerPaymentHistory(string passengerId)
	{
		return await _dataAccessHelper.QueryData<PassengerPaymentHistoriesModel, dynamic>("USP_Passenger_Payment_History_File", new { passengerId = passengerId });
	}
	public async Task<List<PassengerModel>> Filter(string StatusId)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);	

		var output = await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_Filter", p);

		return output;
	}
	public async Task<List<PassengerModel>> FilterWithCity_N_ContactNo(string StatusId, string City, string ContactNo)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("City", City);
		p.Add("ContactNo", ContactNo);

		var output = await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_FilterWithCity_N_ContactNo", p);

		return output;
	}	
    public async Task<List<PassengerModel>> FilterWithCity(string StatusId, string City)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("City", City);

		var output = await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_FilterWithCity", p);

		return output;
	}	
    public async Task<List<PassengerModel>> FilterWithContactNo(string StatusId, string ContactNo)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("ContactNo", ContactNo);

		var output = await _dataAccessHelper.QueryData<PassengerModel, dynamic>("USP_Passenger_FilterWithContactNo", p);

		return output;
	}	
	public async Task<UserInfoModel> Login(UserLoginModel userLogin)
    {
        var result = await _signInManager.PasswordSignInAsync(userLogin.UserName, userLogin.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(userLogin.UserName);
            UserInfoModel userInfo = new UserInfoModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.FullName,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).First()
            };

            return userInfo;
        }
        else if (result.RequiresTwoFactor)
        {
            return null;
        }
        else if (result.IsLockedOut)
        {
            return null;
        }
        else
        {
            return null;
        }
    }
    public async Task<UserInfoModel> ChangePassword(ChangePasswordModel changePassword)
    {
        try
        {
            var applicationUser = await _userManager.FindByIdAsync(changePassword.UserId);

            if (applicationUser != null)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(applicationUser, changePassword.CurrentPassword, changePassword.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(applicationUser);
                    _logger.LogInformation("User changed their password successfully.");



                    #region Audit Log
                    var log = new LogModel
                    {
                        UserName = _context.User.Identity.Name == null ? applicationUser.UserName : _context.User.Identity.Name,
                        UserRole = _context.User.Identity.Name == null ? "SystemAdmin" : _context.User.Claims.First(c => c.Type.Contains("role")).Value,
                        IP = Utility.GetIPAddress(_context.Request),
                        TableName = "AspNetUsers",
                        OldData = _context.User.Identity.Name == null ? null : $"<deleted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Register\" />",
                        NewData = $"<inserted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Register\" />"
                    };
                    _ = Task.Run(async () => { await _auditLogRepository.InsertAuditLog(log); });
                    #endregion


                    #region Send Email
                    var emailTemplate = await _emailTemplateRepository.GetEmailTemplateByName("Reset Password");
                    emailTemplate.Template = emailTemplate.Template.Replace("$fullName", applicationUser.FullName);
                    emailTemplate.Template = emailTemplate.Template.Replace("$password", changePassword.NewPassword);

                    _ = Task.Run(async () =>
                    {
                        await _emailSender.SendEmail(new EmailModel { To = applicationUser.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                    });
                    #endregion

                    UserInfoModel userInfo = new UserInfoModel
                    {
                        Id = applicationUser.Id,
                        UserName = applicationUser.UserName,
                        Name = applicationUser.FullName,
                        Email = applicationUser.Email,
                        Role = _context.User.Identity.Name == null ? "SystemAdmin" : _context.User.Claims.First(c => c.Type.Contains("role")).Value,
                    };

                    return userInfo;
                }
                else

                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        catch(Exception ex)
        {
            throw;
        }
    }
    public async Task UpdateRefreshToken(string userId, TokenModel token)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("UserId", userId);
        p.Add("RefreshToken", token.RefreshToken);
        p.Add("RefreshTokenExpires", token.RefreshTokenExpires);

        await _dataAccessHelper.ExecuteData("USP_AspNetUsers_TokenUpdate", p);
    }
    public async Task<TokenModel> GetRefreshToken(string userId)
    {
        return (await _dataAccessHelper.QueryData<TokenModel, dynamic>("USP_AspNetUsers_GetRefreshToken", new { UserId = userId })).FirstOrDefault();
    }
    public async Task<UserInfoModel> GetCurrentUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        UserInfoModel userInfo = new UserInfoModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.FullName,
            Email = user.Email,
            Role = (await _userManager.GetRolesAsync(user)).First()
        };

        return userInfo;
    }

    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DistinctPassengerAuthCache:
                _cache.Remove(DistinctPassengerAuthCache);
                break;
            case PassengerAuthCache:
                var keys = _cache.Get<List<string>>(PassengerAuthCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PassengerAuthCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}