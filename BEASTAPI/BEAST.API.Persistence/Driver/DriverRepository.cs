using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Driver;
using BEASTAPI.Core.Contract.Driver;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BEASTAPI.Infrastructure;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Model.Passenger;
using BEASTAPI.Core.ViewModel;

namespace BEASTAPI.Persistence;

public class DriverRepository : IDriverRepository
{
    private readonly ILogger<DriverRepository> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HttpContext _context;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly ISMSSender _smsSender;
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private const string DriverCache = "DriverData";
    private const string DriversWithVehicle = "DriversWithVehicleData";
    private const string DistinctDriverCache = "DistinctDriverData";
    private const string DriverFilterValue = "DriverFilterValue";

    public DriverRepository(ILogger<DriverRepository> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor, IAuditLogRepository auditLogRepository, IEmailSender emailSender, IEmailTemplateRepository emailTemplateRepository, IMemoryCache cache, IConfiguration config, IDataAccessHelper dataAccessHelper, ISMSSender smsSender)
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
        this._smsSender = smsSender;
    }

    #region "DataAccessHelper Methods"
    public async Task<RegisterResponseModel> Register(DriverModel driverModel, LogModel logModel)
    {
        ApplicationUser applicationUser = new ApplicationUser
        {
            FullName = driverModel.FirstName + " " + driverModel.MiddleName + " " + driverModel.LastName,
            UserName = string.IsNullOrEmpty(driverModel.Email) ? "" : driverModel.Email,
            EmailConfirmed = !string.IsNullOrEmpty(driverModel.Email) ? true : false,
            UserType = driverModel.UserType,
            PhoneNumberConfirmed = !string.IsNullOrEmpty(driverModel.MobileNumber) ? true : false,
            TwoFactorEnabled = true,
            Email = string.IsNullOrEmpty(driverModel.Email) ? "" : driverModel.Email,
            PhoneNumber = driverModel.MobileNumber,
            LockoutEnabled = true,
            AccessFailedCount = 1
        };

        //var result = await _userManager.CreateAsync(applicationUser, userInfo.Password);
        try
        {
            var result = await _userManager.CreateAsync(applicationUser);


            if (result.Succeeded)
            {
                //await _userManager.AddToRoleAsync(applicationUser, userInfo.Role);
                await _userManager.AddClaimAsync(applicationUser, new System.Security.Claims.Claim("FullName", driverModel.FirstName + " " + driverModel.MiddleName + " " + driverModel.LastName));

                #region Applicaion Log
                _logger.LogInformation("User created a new account with First Name, Middle Name & Last Name for Driver.");
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

                driverModel.UserId = applicationUser.Id;
                driverModel.CreatedDate = DateTime.Now;
                driverModel.Id = Guid.NewGuid().ToString();
                string insertedPassengerId = await InsertDriver(driverModel, log);


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
    public async Task<RegisterResponseModel> SendOTP(string id, DriverModel driverModel, string generatedOTP)
    {
        ApplicationUser applicationUser = new ApplicationUser
        {
            UserName = driverModel.Email,
            EmailConfirmed = !string.IsNullOrEmpty(driverModel.Email) ? true : false,
            UserType = driverModel.UserType,
            PhoneNumberConfirmed = !string.IsNullOrEmpty(driverModel.MobileNumber) ? true : false,
            TwoFactorEnabled = true,
            Email = driverModel.Email,
            PhoneNumber = driverModel.MobileNumber,
            LockoutEnabled = true,
            AccessFailedCount = 1
        };

        try
        {
            //var result = await _userManager.UpdateAsync(applicationUser);


            //if (result.Succeeded)
            //{
            //    await _userManager.AddClaimAsync(applicationUser, new System.Security.Claims.Claim("FullName", driverModel.FirstName + " " + driverModel.MiddleName + " " + driverModel.LastName));

            //    #region Applicaion Log
            //    _logger.LogInformation("User created a new account through SendOTP in Driver.");
            //    #endregion

            //    #region Audit Log
            var log = new LogModel
            {
                UserName = _context.User.Identity.Name == null ? applicationUser.UserName : _context.User.Identity.Name,
                UserRole = _context.User.Identity.Name == null ? "SystemAdmin" : _context.User.Claims.First(c => c.Type.Contains("role")).Value,
                IP = Utility.GetIPAddress(_context.Request)
                // TableName = "AspNetUsers",
                // OldData = _context.User.Identity.Name == null ? null : $"<SendOTP Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"DriverSendOTP\" />",
                // NewData = $"<inserted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"DriverSendOTP\" />"
            };
            //    //_ = Task.Run(async () => { await _auditLogRepository.InsertAuditLog(log); });
            //    #endregion

            //    driverModel.UserId = applicationUser.Id;
            //    driverModel.Email = applicationUser.Email;
            //    driverModel.MobileNumber = applicationUser.PhoneNumber;
            //    driverModel.Id = driverModel.Id;
            //    await UpdateDriver(driverModel, log);


            driverModel.OtpCode = generatedOTP;
            driverModel.StartTime = DateTime.Now;
            driverModel.EndTime = DateTime.Now.AddMinutes(10);
            driverModel.TimeInMinutes = 10;
            await InsertOTP(driverModel, log);


            if (!string.IsNullOrEmpty(driverModel.MobileNumber))
            {
                List<string> lstMobileNo = new List<string>();
                lstMobileNo.Add(driverModel.MobileNumber);
                SMSModel model = new SMSModel();
                model.To = lstMobileNo;
                model.Content = generatedOTP + " is your One-Time Password, valid for 10 minutes only, Please do not share your OTP with anyone.";

                await Task.Run(() => _smsSender.SendSMS(model));
            }
            //else
            //{
            //    #region Send Email

            //    //TODO: Make URL come from settings, also check if email confirmation works
            //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //    var callbackUrl = $@"http://localhost:5122/Identity/Account/ConfirmEmail?userId={applicationUser.Id}&code={code}";
            //    callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

            //    var emailTemplate = await _emailTemplateRepository.GetEmailTemplateByName("OTP Email");
            //    emailTemplate.Template = emailTemplate.Template.Replace("OTP", generatedOTP);

            //    _ = Task.Run(async () =>
            //    {
            //        await _emailSender.SendEmail(new EmailModel { To = driverModel.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
            //    });
            //    #endregion                   
            //}
            //}            
        }
        catch (Exception ex)
        {
            throw;
        }
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

    public async Task<DriverModel> ValidateDriverOTP(DriverModel driverModel)
    {
        return (await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_OTP_Validate", new { UserId = driverModel.UserId, OtpCode = driverModel.OtpCode })).SingleOrDefault();
    }

    public async Task<PaginatedListModel<DriverModel>> GetDrivers(int pageNumber)
    {
        PaginatedListModel<DriverModel> output = _cache.Get<PaginatedListModel<DriverModel>>(DriverCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_Driver_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<DriverModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(DriverCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(DriverCache);
            if (keys is null)
                keys = new List<string> { DriverCache + pageNumber };
            else
                keys.Add(DriverCache + pageNumber);
            _cache.Set(DriverCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

	public async Task<PaginatedListModel<DriverModel>> GetDrivers(int pageNumber, bool IsApproved, string StatusId, string NID, string DrivingLicenseNo)
	{
		PaginatedListModel<DriverModel> output = _cache.Get<PaginatedListModel<DriverModel>>(DriverCache + pageNumber+ IsApproved+ StatusId+ NID+ DrivingLicenseNo);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("IsApproved", IsApproved);
			p.Add("StatusId", StatusId);
			p.Add("NID", NID);
			p.Add("DrivingLicenseNo", DrivingLicenseNo);
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_Driver_GetAllStatusWise", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<DriverModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(DriverCache + pageNumber + IsApproved + StatusId + NID + DrivingLicenseNo, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(DriverCache);
			if (keys is null)
				keys = new List<string> { DriverCache + pageNumber };
			else
				keys.Add(DriverCache + pageNumber);
			_cache.Set(DriverCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<DriverModel> GetDriverById(string DriverId)
    {
        return (await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_Driver_GetById", new { Id = DriverId })).FirstOrDefault();
    }

    public async Task<List<DriverModel>> GetDistinctDrivers()
    {
        var output = _cache.Get<List<DriverModel>>(DistinctDriverCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_Driver_GetDistinct", new { });
            _cache.Set(DistinctDriverCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }   public async Task<List<DriverModel>> GetActiveDrivers()
    {
        var output = _cache.Get<List<DriverModel>>(DistinctDriverCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_Get_ActiveDrivers", new { });
            _cache.Set(DistinctDriverCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }  
    public async Task<List<DriverModel>> Filter(bool IsApproved, string StatusId, string NID, string DrivingLicenseNo)
    {
    
			DynamicParameters p = new DynamicParameters();
			p.Add("StatusId", StatusId);
			p.Add("NID", NID);
			p.Add("DrivingLicenseNo", DrivingLicenseNo);
			p.Add("IsApproved", IsApproved);
			
		var output = await _dataAccessHelper.QueryData<DriverModel, dynamic>("Filter_Drivers", p);
      
        return output;
    }
    public async Task<string> InsertDriver(DriverModel driver, LogModel logModel)
    {
        ClearCache(DriverCache);
        ClearCache(DriversWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", driver.Id);
        p.Add("UserId", driver.UserId);
        p.Add("FirstName", driver.FirstName);
        p.Add("MiddleName", driver.MiddleName);
        p.Add("LastName", driver.LastName);
        p.Add("DateOfBirth", driver.DateOfBirth);
        p.Add("Email", driver.Email);
        p.Add("MobileNumber", driver.MobileNumber);
        p.Add("GenderId", driver.GenderId);
        p.Add("NID", driver.NID);
        p.Add("DrivingLicenseNo", driver.DrivingLicenseNo);
        p.Add("IsApproved", driver.IsApproved);
        p.Add("ApprovedBy", driver.ApprovedBy);
        p.Add("ApprovedDate", driver.ApprovedDate);
        p.Add("CreatedBy", driver.CreatedBy);
        p.Add("IsActive", driver.IsActive);
        p.Add("IsDeleted", driver.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Driver_Insert", p);
        return driver.Id;
    }

    public async Task UpdateDriver(DriverModel driver, LogModel logModel)
    {
        ClearCache(DriverCache);
        ClearCache(DriversWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", driver.Id);
        p.Add("UserId", driver.UserId);
        p.Add("FirstName", driver.FirstName);
        p.Add("MiddleName", driver.MiddleName);
        p.Add("LastName", driver.LastName);
        p.Add("DateOfBirth", driver.DateOfBirth);
        p.Add("Email", driver.Email);
        p.Add("MobileNumber", driver.MobileNumber);
        p.Add("GenderId", driver.GenderId);
        p.Add("NID", driver.NID);
        p.Add("DrivingLicenseNo", driver.DrivingLicenseNo);
		
		p.Add("IsApproved", driver.IsApproved);
        p.Add("ApprovedBy", driver.ApprovedBy);
        p.Add("ApprovedDate", driver.ApprovedDate);
        p.Add("ModifiedBy", driver.ModifiedBy);
        p.Add("IsActive", driver.IsActive);
        p.Add("IsDeleted", driver.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Driver_Update", p);
    }  
    public async Task UpdateStatus(DriverModel driver, LogModel logModel)
    {
        ClearCache(DriverCache);
        ClearCache(DriversWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", driver.Id);
        p.Add("StatusId", driver.StatusId);

        p.Add("ModifiedBy", driver.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Driver_UpdateStatus", p);
    }

    public async Task DeleteDriver(string DriverId, LogModel logModel)
    {
        ClearCache(DriverCache);
        ClearCache(DriversWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", DriverId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Driver_Delete", p);
    }


    public async Task<PaginatedListModel<DriverCommissionModel>> GetDriverCommissions(string driverId, int pageNumber)
    {
        PaginatedListModel<DriverCommissionModel> output = _cache.Get<PaginatedListModel<DriverCommissionModel>>(DriverCache+ driverId + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("DriverId", driverId);
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<DriverCommissionModel, dynamic>("USP_Driver_GetAllCommissions", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<DriverCommissionModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(DriverCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(DriverCache);
            if (keys is null)
                keys = new List<string> { DriverCache + pageNumber };
            else
                keys.Add(DriverCache + pageNumber);
            _cache.Set(DriverCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<List<DriverModel>> Export(string StatusId, string NID, string DrivingLicenseNo, bool IsApproved)
	{
		//return await _dataAccessHelper.QueryData<DriverModel, dynamic>("USP_Driver_Export", new { IsApproved });

		DynamicParameters p = new DynamicParameters();
		p.Add("StatusId", StatusId);
		p.Add("NID", NID);
		p.Add("DrivingLicenseNo", DrivingLicenseNo);
		p.Add("IsApproved", IsApproved);

		return  await _dataAccessHelper.QueryData<DriverModel, dynamic>("Filter_Drivers", p);
	}
    #endregion

    #region "Customized Methods"

    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DistinctDriverCache:
                _cache.Remove(DistinctDriverCache);
                break;
            case DriverCache:
                var keys = _cache.Get<List<string>>(DriverCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(DriverCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion


    #region OTP insertion
    public async Task<int> InsertOTP(DriverModel driverModel, LogModel logModel)
    {
        try
        {
            ClearCache(DriverCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", 404);
            p.Add("UserId", driverModel.UserId);
            p.Add("OtpCode", driverModel.OtpCode);
            p.Add("StartTime", driverModel.StartTime);
            p.Add("EndTime", driverModel.EndTime);
            p.Add("TimeInMinutes", driverModel.TimeInMinutes);

            p.Add("IsActive", driverModel.IsActive);
            p.Add("IsDeleted", driverModel.IsDeleted);
            p.Add("CreatedBy", driverModel.CreatedBy);
            p.Add("CreatedDate", driverModel.CreatedDate);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);

            await _dataAccessHelper.ExecuteData("USP_OTP_Insert", p);
            return 0;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    #endregion
}