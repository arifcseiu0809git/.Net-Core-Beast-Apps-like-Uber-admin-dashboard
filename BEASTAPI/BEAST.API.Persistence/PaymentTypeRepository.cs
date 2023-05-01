using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence;

public class PaymentTypeRepository : IPaymentTypeRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string PaymentTypeCache = "PaymentTypeData";
    private const string DistinctPaymentTypeCache = "DistinctPaymentTypeCacheData";
    
    public PaymentTypeRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<PaymentTypeModel>> GetPaymentTypes(int pageNumber)
    {
        PaginatedListModel<PaymentTypeModel> output = _cache.Get<PaginatedListModel<PaymentTypeModel>>(PaymentTypeCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PaymentTypeModel, dynamic>("USP_PaymentType_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<PaymentTypeModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(PaymentTypeCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(PaymentTypeCache);
            if (keys is null)
                keys = new List<string> { PaymentTypeCache + pageNumber };
            else
                keys.Add(PaymentTypeCache + pageNumber);
            _cache.Set(PaymentTypeCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }


    public async Task<List<PaymentTypeModel>> GetDistinctPaymentTypes()
    {
        var output = _cache.Get<List<PaymentTypeModel>>(DistinctPaymentTypeCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<PaymentTypeModel, dynamic>("USP_PaymentType_GetDistinct", new { });
            _cache.Set(DistinctPaymentTypeCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<PaymentTypeModel> GetPaymentTypeById(string paymentTypeId)
    {
        return (await _dataAccessHelper.QueryData<PaymentTypeModel, dynamic>("USP_PaymentType_GetById", new { Id = paymentTypeId })).FirstOrDefault();
    }

    public async Task<PaymentTypeModel> GetPaymentTypeByName(string paymentTypeName)
    {
        return (await _dataAccessHelper.QueryData<PaymentTypeModel, dynamic>("USP_PaymentType_GetByName", new { Name = paymentTypeName })).FirstOrDefault();
    }

    public async Task<string> InsertPaymentType(PaymentTypeModel paymentType, LogModel logModel)
    {
        ClearCache(PaymentTypeCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentType.Id);
        p.Add("Name", paymentType.Name);
        p.Add("IsActive", paymentType.IsActive);
        p.Add("IsDeleted", paymentType.IsDeleted);
        p.Add("CreatedBy", paymentType.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentType_Insert", p);
        return paymentType.Id;
    }

    public async Task UpdatePaymentType(PaymentTypeModel paymentType, LogModel logModel)
    {
        ClearCache(PaymentTypeCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentType.Id); 
        p.Add("Name", paymentType.Name); 
        p.Add("IsActive", paymentType.IsActive);
        p.Add("IsDeleted", paymentType.IsDeleted);
        p.Add("ModifiedBy", paymentType.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentType_Update", p);

    }

    public async Task DeletePaymentType(string paymentTypeId, LogModel logModel)
    {
        ClearCache(PaymentTypeCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentTypeId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentType_Delete", p);
    }

    public async Task<List<PaymentTypeModel>> Export()
    {
        return await _dataAccessHelper.QueryData<PaymentTypeModel, dynamic>("USP_PaymentType_Export", new { });
    }

    public async Task<bool> CheckIfDuplicateExists(string id, string paymentTypeName) 
    {
        DynamicParameters p = new DynamicParameters(); 
        p.Add("Id", id);
        p.Add("Name", paymentTypeName);
        return (await _dataAccessHelper.QueryData<bool, dynamic>("USP_PaymentType_CheckIfDuplicateExists", p)).FirstOrDefault();
    }

    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case PaymentTypeCache:
                var keys = _cache.Get<List<string>>(PaymentTypeCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PaymentTypeCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}