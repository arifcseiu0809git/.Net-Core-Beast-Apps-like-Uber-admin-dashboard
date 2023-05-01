using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Passenger;

public class PassengersPaymentMethodRepository : IPassengersPaymentMethodRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string PassengersPaymentMethodCache = "PassengersPaymentMethodData";

    public PassengersPaymentMethodRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<PassengersPaymentMethodModel>> GetPassengersPaymentMethods(int pageNumber)
    {
        PaginatedListModel<PassengersPaymentMethodModel> output = _cache.Get<PaginatedListModel<PassengersPaymentMethodModel>>(PassengersPaymentMethodCache + pageNumber);
        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PassengersPaymentMethodModel, dynamic>("USP_PassengerPaymentMethod_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));
            output = new PaginatedListModel<PassengersPaymentMethodModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };
            _cache.Set(PassengersPaymentMethodCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            List<string> keys = _cache.Get<List<string>>(PassengersPaymentMethodCache);
            if (keys is null)
                keys = new List<string> { PassengersPaymentMethodCache + pageNumber };
            else
                keys.Add(PassengersPaymentMethodCache + pageNumber);
            _cache.Set(PassengersPaymentMethodCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }
        return output;
    }
    public async Task<PassengersPaymentMethodModel> GetPassengersPaymentMethodById(string passengersPaymentMethodId)
    {
        return (await _dataAccessHelper.QueryData<PassengersPaymentMethodModel, dynamic>("USP_PassengerPaymentMethod_GetById", new { Id = passengersPaymentMethodId })).SingleOrDefault();
    }
    public async Task<PassengersPaymentMethodModel> GetPassengersPaymentMethodByName(string passengersPaymentMethodName)
    {
        return (await _dataAccessHelper.QueryData<PassengersPaymentMethodModel, dynamic>("USP_PassengersPaymentMethod_GetByName", new { Name = passengersPaymentMethodName })).SingleOrDefault();
    }
    public async Task<string> InsertPassengersPaymentMethod(PassengersPaymentMethodModel passengersPaymentMethod, LogModel logModel)
    {
        ClearCache(PassengersPaymentMethodCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", passengersPaymentMethod.Id);
        p.Add("PassengerId", passengersPaymentMethod.PassengerId);
        p.Add("PaymentMethodName", passengersPaymentMethod.PaymentMethodName);
        p.Add("PaymentMethodType", passengersPaymentMethod.PaymentMethodType);
        p.Add("Password", passengersPaymentMethod.Password);
        p.Add("AccountNumber", passengersPaymentMethod.AccountNumber);
        p.Add("AccountType", passengersPaymentMethod.AccountType);
        p.Add("ExpiredOnMonth", passengersPaymentMethod.ExpiredOnMonth);
        p.Add("ExpiredOnYear", passengersPaymentMethod.ExpiredOnYear);
        p.Add("Cvv", passengersPaymentMethod.Cvv);
        p.Add("CountryName", passengersPaymentMethod.CountryName);
        p.Add("SortingPriority", passengersPaymentMethod.SortingPriority);
        p.Add("PaymentMethodTypeIcon", passengersPaymentMethod.PaymentMethodTypeIcon);
        //p.Add("IsActive", passengersPaymentMethod.IsActive);
        //p.Add("IsLocked", passengersPaymentMethod.IsLocked);
        //p.Add("IsDeleted", passengersPaymentMethod.IsDeleted);
        p.Add("CreatedDate", DateTime.Now);
        p.Add("ModifiedDate", DateTime.Now);
        p.Add("CreatedBy", passengersPaymentMethod.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PassengerPaymentMethod_Insert", p);
        return passengersPaymentMethod.Id;
    }
    public async Task UpdatePassengersPaymentMethod(PassengersPaymentMethodModel passengersPaymentMethod, LogModel logModel)
    {
        ClearCache(PassengersPaymentMethodCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("PassengerId", passengersPaymentMethod.PassengerId);
        p.Add("PaymentMethodName", passengersPaymentMethod.PaymentMethodName);
        p.Add("PaymentMethodType", passengersPaymentMethod.PaymentMethodType);
        p.Add("Password", passengersPaymentMethod.Password);
        p.Add("AccountNumber", passengersPaymentMethod.AccountNumber);
        p.Add("AccountType", passengersPaymentMethod.AccountType);
        p.Add("ExpiredOnMonth", passengersPaymentMethod.ExpiredOnMonth);
        p.Add("ExpiredOnYear", passengersPaymentMethod.ExpiredOnYear);
        p.Add("Cvv", passengersPaymentMethod.Cvv);
        p.Add("CountryName", passengersPaymentMethod.CountryName);
        p.Add("SortingPriority", passengersPaymentMethod.SortingPriority);
        p.Add("PaymentMethodTypeIcon", passengersPaymentMethod.PaymentMethodTypeIcon);
        p.Add("Id", passengersPaymentMethod.Id);
        //p.Add("IsActive", passengersPaymentMethod.IsActive);
        //p.Add("IsLocked", passengersPaymentMethod.IsLocked);
        //p.Add("IsDeleted", passengersPaymentMethod.IsDeleted);
        //p.Add("ModifiedBy", passengersPaymentMethod.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PassengerPaymentMethod_Update", p);
    }
    public async Task DeletePassengersPaymentMethod(string passengersPaymentMethodId, LogModel logModel)
    {
        ClearCache(PassengersPaymentMethodCache);
        DynamicParameters p = new DynamicParameters();
        p.Add("Id", passengersPaymentMethodId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PassengerPaymentMethod_Delete", p);
    }
    public async Task<List<PassengersPaymentMethodModel>> Export()
    {
        return await _dataAccessHelper.QueryData<PassengersPaymentMethodModel, dynamic>("USP_PassengerPaymentMethod_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case PassengersPaymentMethodCache:
                var keys = _cache.Get<List<string>>(PassengersPaymentMethodCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PassengersPaymentMethodCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}