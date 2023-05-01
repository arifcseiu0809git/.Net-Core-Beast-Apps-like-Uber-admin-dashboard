using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using System.Data;

namespace BEASTAPI.Persistence;

public class SMSRepository : ISMSRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string SMSCache = "SMSData";

    public SMSRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<PaginatedListModel<SMSModels>> GetSMSes(int pageNumber)
    {
        PaginatedListModel<SMSModels> output = _cache.Get<PaginatedListModel<SMSModels>>(SMSCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<SMSModels, dynamic>("USP_SMS_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<SMSModels>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(SMSCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(SMSCache);
            if (keys is null)
                keys = new List<string> { SMSCache + pageNumber };
            else
                keys.Add(SMSCache + pageNumber);
            _cache.Set(SMSCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<SMSModels> GetSMSById(string smsId)
    {
        return (await _dataAccessHelper.QueryData<SMSModels, dynamic>("USP_SMS_GetById", new { Id = smsId })).FirstOrDefault();
    }

    public async Task<string> InsertSMS(SMSModels sms, LogModel logModel)
    {
        try
        {
            ClearCache(SMSCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", sms.Id);
            p.Add("UserId", sms.UserId);
            p.Add("BodyText", sms.BodyText);
            p.Add("IsSuccessfullySend", sms.IsSuccessfullySend);
            p.Add("FailReason", sms.FailReason);
            p.Add("IsActive", sms.IsActive);
            p.Add("IsDeleted", sms.IsDeleted);
            p.Add("CreatedBy", sms.CreatedBy);
            p.Add("CreatedDate", sms.CreatedDate);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_SMS_Insert", p);
            return sms.Id;
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    public async Task UpdateSMS(SMSModels sms, LogModel logModel)
    {
        ClearCache(SMSCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", sms.Id);
        p.Add("UserId", sms.UserId);
        p.Add("BodyText", sms.BodyText);
        p.Add("IsSuccessfullySend", sms.IsSuccessfullySend);
        p.Add("FailReason", sms.FailReason);
        p.Add("IsActive", sms.IsActive);
        p.Add("IsDeleted", sms.IsDeleted);
        p.Add("ModifiedBy", sms.ModifiedBy);
        p.Add("ModifiedDate", sms.ModifiedDate);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SMS_Update", p);
    }

    public async Task DeleteSMS(string smsId, LogModel logModel)
    {
        ClearCache(SMSCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", smsId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SMS_Delete", p);
    }

    public async Task<List<SMSModels>> Export()
    {
        return await _dataAccessHelper.QueryData<SMSModels, dynamic>("USP_SMS_Export", new { });
    }

    private void ClearCache(string key)
    {
        switch (key)
        {
            case SMSCache:
                var keys = _cache.Get<List<string>>(SMSCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(SMSCache);
                }
                break;
            default:
                break;
        }
    }
}