using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence;

public class PaymentOptionRepository : IPaymentOptionRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string PaymentOptionCache = "PaymentOptionData";
    private const string DistinctPaymentOptionCache = "DistinctPaymentOptionCacheData";

    

    public PaymentOptionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<PaymentOptionModel>> GetPaymentOption(int pageNumber)
    {
        PaginatedListModel<PaymentOptionModel> output = _cache.Get<PaginatedListModel<PaymentOptionModel>>(PaymentOptionCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PaymentOptionModel, dynamic>("USP_PaymentOption_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<PaymentOptionModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(PaymentOptionCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(PaymentOptionCache);
            if (keys is null)
                keys = new List<string> { PaymentOptionCache + pageNumber };
            else
                keys.Add(PaymentOptionCache + pageNumber);
            _cache.Set(PaymentOptionCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<List<PaymentOptionModel>> GetDistinctPaymentOptions()
    {
        var output = _cache.Get<List<PaymentOptionModel>>(DistinctPaymentOptionCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<PaymentOptionModel, dynamic>("USP_PaymentOption_GetDistinct", new { });
            _cache.Set(DistinctPaymentOptionCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
    public async Task<PaymentOptionModel> GetPaymentOptionById(string paymentOptionId)
    {
        return (await _dataAccessHelper.QueryData<PaymentOptionModel, dynamic>("USP_PaymentOption_GetById", new { Id = paymentOptionId })).FirstOrDefault();
    }

    public async Task<PaymentOptionModel> GetPaymentOptionByName(string paymentOptionName)
    {
        return (await _dataAccessHelper.QueryData<PaymentOptionModel, dynamic>("USP_PaymentOption_GetByName", new { Name = paymentOptionName })).FirstOrDefault();
    }

    public async Task<string> InsertPaymentOption(PaymentOptionModel paymentOption, LogModel logModel)
    {
        ClearCache(PaymentOptionCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentOption.Id);
        p.Add("PaymentType", paymentOption.PaymentType);
        p.Add("Name", paymentOption.Name);
        p.Add("IsActive", paymentOption.IsActive);
        p.Add("IsDeleted", paymentOption.IsDeleted);
        p.Add("CreatedBy", paymentOption.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentOption_Insert", p);
        return paymentOption.Id;
    }

    public async Task UpdatePaymentOption(PaymentOptionModel paymentOption, LogModel logModel)
    {
        ClearCache(PaymentOptionCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentOption.Id);
        p.Add("PaymentType", paymentOption.PaymentType);
        p.Add("Name", paymentOption.Name); 
        p.Add("IsActive", paymentOption.IsActive);
        p.Add("IsDeleted", paymentOption.IsDeleted);
        p.Add("ModifiedBy", paymentOption.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentOption_Update", p);

    }

    public async Task DeletePaymentOption(string paymentOptionId, LogModel logModel)
    {
        ClearCache(PaymentOptionCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentOptionId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentOption_Delete", p);
    }

    public async Task<List<PaymentOptionModel>> Export()
    {
        return await _dataAccessHelper.QueryData<PaymentOptionModel, dynamic>("USP_PaymentOption_Export", new { });
    }

    public async Task<List<PaymentOptionModel>> GetPaymentOptionsByPaymentTypeId(string paymentTypeId)
    {
        ClearCache(PaymentOptionCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("paymentTypeId", paymentTypeId);
        var result = await _dataAccessHelper.QueryData<PaymentOptionModel, dynamic>("USP_PaymentOption_GetByPaymentTypeId", p);
        return result;
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case PaymentOptionCache:
                var keys = _cache.Get<List<string>>(PaymentOptionCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PaymentOptionCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}