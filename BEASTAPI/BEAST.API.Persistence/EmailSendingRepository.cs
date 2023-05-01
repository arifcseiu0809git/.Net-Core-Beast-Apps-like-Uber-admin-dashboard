using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence;

public class EmailSendingRepository : IEmailSendingRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string EmailSendingCache = "EmailSendingData";

    public EmailSendingRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<EmailSendingModel>> GetEmailSendings(int pageNumber)
    {
        PaginatedListModel<EmailSendingModel> output = _cache.Get<PaginatedListModel<EmailSendingModel>>(EmailSendingCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<EmailSendingModel, dynamic>("USP_Email_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<EmailSendingModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(EmailSendingCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(EmailSendingCache);
            if (keys is null)
                keys = new List<string> { EmailSendingCache + pageNumber };
            else
                keys.Add(EmailSendingCache + pageNumber);
            _cache.Set(EmailSendingCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<EmailSendingModel> GetEmailSendingById(string emailSendingId)
    {
        return (await _dataAccessHelper.QueryData<EmailSendingModel, dynamic>("USP_Email_GetById", new { Id = emailSendingId })).FirstOrDefault();
    }

    public async Task<string> InsertEmailSending(EmailSendingModel emailSending, LogModel logModel)
    {
        ClearCache(EmailSendingCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", emailSending.Id);
        p.Add("UserId", emailSending.UserId);
        p.Add("EmailBody", emailSending.EmailBody);
        p.Add("IsSuccessfullySend", emailSending.IsSuccessfullySend);
        p.Add("FailReason", emailSending.FailReason);
        p.Add("IsActive", emailSending.IsActive);
        p.Add("IsDeleted", emailSending.IsDeleted);
        p.Add("CreatedBy", emailSending.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Email_Insert", p);
        return emailSending.Id;
    }

    public async Task UpdateEmailSending(EmailSendingModel emailSending, LogModel logModel)
    {
        ClearCache(EmailSendingCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", emailSending.Id);
        p.Add("UserId", emailSending.UserId);
        p.Add("EmailBody", emailSending.EmailBody);
        p.Add("IsSuccessfullySend", emailSending.IsSuccessfullySend);
        p.Add("FailReason", emailSending.FailReason);
        p.Add("IsActive", emailSending.IsActive);
        p.Add("IsDeleted", emailSending.IsDeleted);
        p.Add("ModifiedBy", emailSending.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Email_Update", p);

    }

    public async Task DeleteEmailSending(string EmailSendingId, LogModel logModel)
    {
        ClearCache(EmailSendingCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", EmailSendingId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Email_Delete", p);
    }

    public async Task<List<EmailSendingModel>> Export()
    {
        return await _dataAccessHelper.QueryData<EmailSendingModel, dynamic>("USP_Email_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case EmailSendingCache:
                var keys = _cache.Get<List<string>>(EmailSendingCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(EmailSendingCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}