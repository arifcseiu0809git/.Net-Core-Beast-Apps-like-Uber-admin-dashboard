using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Common;

public class PricingRepository : IPricingRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string PricingCache = "PricingData";

    public PricingRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<PricingModel>> GetPricings(int pageNumber)
    {
        PaginatedListModel<PricingModel> output = _cache.Get<PaginatedListModel<PricingModel>>(PricingCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PricingModel, dynamic>("USP_Pricing_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<PricingModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(PricingCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(PricingCache);
            if (keys is null)
                keys = new List<string> { PricingCache + pageNumber };
            else
                keys.Add(PricingCache + pageNumber);
            _cache.Set(PricingCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<PricingModel> GetPricingById(string locationId)
    {
        return (await _dataAccessHelper.QueryData<PricingModel, dynamic>("USP_Pricing_GetById", new { Id = locationId })).FirstOrDefault();
    }


    public async Task<PricingModel> GetPricingByName(string locationName)
    {
        return (await _dataAccessHelper.QueryData<PricingModel, dynamic>("USP_Pricing_GetByName", new { Name = locationName })).FirstOrDefault();
    }

    public async Task<string> InsertPricing(PricingModel price, LogModel logModel)
    {
        ClearCache(PricingCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", price.Id);
        p.Add("VehicleTypeId", price.VehicleTypeId);
        p.Add("BaseFare", price.BaseFare);
        p.Add("BookingFee", price.BookingFee);
        p.Add("CostPerMin", price.CostPerMin);
        p.Add("CostPerKm", price.CostPerKm);
        p.Add("MinCharge", price.MinCharge);
        p.Add("CancelFee", price.CancelFee);
        p.Add("CurrencyName", price.CurrencyName);
        p.Add("DistrictId", price.DistrictId);
        p.Add("CreatedBy", price.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Pricing_Insert", p);
        return price.Id;
    }

    public async Task UpdatePricing(PricingModel location, LogModel logModel)
    {
        ClearCache(PricingCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", location.Id);
        p.Add("VehicleTypeId", location.VehicleTypeId);
        p.Add("BaseFare", location.BaseFare);
        p.Add("BookingFee", location.BookingFee);
        p.Add("CostPerMin", location.CostPerMin);
        p.Add("CostPerKm", location.CostPerKm);
        p.Add("MinCharge", location.MinCharge);
        p.Add("CancelFee", location.CancelFee);
        p.Add("CurrencyName", location.CurrencyName);
        p.Add("DistrictId", location.DistrictId);
        p.Add("LastModifiedBy", location.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Pricing_Update", p);
    }

    public async Task DeletePricing(string locationId, LogModel logModel)
    {
        ClearCache(PricingCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", locationId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Pricing_Delete", p);
    }

    public async Task<List<PricingModel>> Export()
    {
        return await _dataAccessHelper.QueryData<PricingModel, dynamic>("USP_Pricing_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case PricingCache:
                var keys = _cache.Get<List<string>>(PricingCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PricingCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}