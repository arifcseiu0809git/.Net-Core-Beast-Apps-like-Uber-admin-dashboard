using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Persistence;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string PaymentMethodCache = "PaymentMethodData";

    public PaymentMethodRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<PaymentMethodModel>> GetPaymentMethods(int pageNumber)
    {
        PaginatedListModel<PaymentMethodModel> output = _cache.Get<PaginatedListModel<PaymentMethodModel>>(PaymentMethodCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("USP_PaymentMethod_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<PaymentMethodModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(PaymentMethodCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(PaymentMethodCache);
            if (keys is null)
                keys = new List<string> { PaymentMethodCache + pageNumber };
            else
                keys.Add(PaymentMethodCache + pageNumber);
            _cache.Set(PaymentMethodCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }


    public async Task<PaymentMethodModel> GetPaymentMethodById(string PaymentMethodId)
    {
        return (await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("USP_PaymentMethod_GetById", new { Id = PaymentMethodId })).FirstOrDefault();
    }

    public async Task<PaymentMethodModel> GetPaymentMethodByName(string PaymentMethodName)
    {
        return (await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("USP_PaymentMethod_GetByName", new { Name = PaymentMethodName })).FirstOrDefault();
    }

    public async Task<string> InsertPaymentMethod(PaymentMethodModel paymentMethod, LogModel logModel)
    {
        ClearCache(PaymentMethodCache);
        try
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("Id", paymentMethod.Id);
            p.Add("UserId", paymentMethod.UserId);
            p.Add("PaymentType", paymentMethod.PaymentType);
            p.Add("PaymentOption", paymentMethod.PaymentOption);
            p.Add("AccountNumber", paymentMethod.AccountNumber);
            p.Add("ExpireMonthYear", paymentMethod.ExpireMonthYear);
            p.Add("CvvCode", paymentMethod.CvvCode);
            p.Add("IsActive", paymentMethod.IsActive);

            p.Add("CreatedBy", paymentMethod.CreatedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            var output = await _dataAccessHelper.ExecuteData("USP_PaymentMethod_Insert", p);
		}
		catch (Exception ex)
		{

			return ex.Message.Split('!')[0];
		}
		return "Sucess";
		
    }

    public async Task<string> UpdatePaymentMethod(PaymentMethodModel paymentMethod, LogModel logModel)
    {
        ClearCache(PaymentMethodCache);
        try
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("Id", paymentMethod.Id);
            p.Add("UserId", paymentMethod.UserId);
            p.Add("PaymentType", paymentMethod.PaymentType);
            p.Add("PaymentOption", paymentMethod.PaymentOption);
            p.Add("AccountNumber", paymentMethod.AccountNumber);
            p.Add("ExpireMonthYear", paymentMethod.ExpireMonthYear);
            p.Add("CvvCode", paymentMethod.CvvCode);
            p.Add("IsActive", paymentMethod.IsActive);
            p.Add("ModifiedBy", paymentMethod.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_PaymentMethod_Update", p);
		}
		catch (Exception ex)
		{

			return ex.Message.Split('!')[0];
		}
		return "Sucess";

	}

    public async Task DeletePaymentMethod(string paymentMethodId, LogModel logModel)
    {
        ClearCache(PaymentMethodCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", paymentMethodId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PaymentMethod_Delete", p);
    }



    public async Task<List<PaymentMethodModel>> Filter(string PaymentType, string PaymentOption, string ContactNo, string AccountNo)
    {

        DynamicParameters p = new DynamicParameters();
        p.Add("PaymentType", PaymentType);
        p.Add("PaymentOption", PaymentOption);
        p.Add("ContactNo", ContactNo);
        p.Add("AccountNo", AccountNo);

        var output = await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("Filter_PaymentMethods", p);

        return output;
    }

    public async Task<List<PaymentMethodModel>> Export()
    {
        return await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("USP_PaymentMethod_Export", new { });
    }

    public async Task<List<PaymentMethodModel>> GetPaymentMethodsByPaymentTypeAndPaymentOption(string paymentTypeId, string paymentOptionId)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("paymentTypeId", paymentTypeId);
        p.Add("paymentOptionId", paymentOptionId);

        var result = await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("USP_PaymentMethod_GetPaymentMethodsByPaymentTypeAndPaymentOption", p);
        return result;
    }    
    public async Task<List<PaymentMethodModel>> GetPaymentMethodByuserId(string paymentTypeId, string paymentOptionId, string userId)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("paymentTypeId", paymentTypeId);
        p.Add("paymentOptionId", paymentOptionId);
        p.Add("userId", userId);

        var result = await _dataAccessHelper.QueryData<PaymentMethodModel, dynamic>("USP_PaymentMethod_GetByUserId", p);
        return result;
    }

    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case PaymentMethodCache:
                var keys = _cache.Get<List<string>>(PaymentMethodCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PaymentMethodCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}