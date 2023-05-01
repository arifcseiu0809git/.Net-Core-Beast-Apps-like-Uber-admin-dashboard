using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Common;

public class CountryRepository : ICountryRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string CountryCache = "CountryData";
    private const string DistinctCountryCache = "DistinctCountryData";

    public CountryRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<CountryModel>> GetCountries(int pageNumber)
    {
        PaginatedListModel<CountryModel> output = _cache.Get<PaginatedListModel<CountryModel>>(CountryCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<CountryModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(CountryCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(CountryCache);
            if (keys is null)
                keys = new List<string> { CountryCache + pageNumber };
            else
                keys.Add(CountryCache + pageNumber);
            _cache.Set(CountryCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<CountryModel> GetCountryById(string countryId)
    {
        return (await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetById", new { Id = countryId })).FirstOrDefault();
    }

    public async Task<List<CountryModel>> GetDistinctCountries()
    {
        try
        {
            var output = _cache.Get<List<CountryModel>>(DistinctCountryCache);

            if (output is null)
            {
                output = await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetDistinct", new { });
                _cache.Set(DistinctCountryCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            }

            return output;
        }
        catch(Exception ex)
        {
            throw;
        }
    }
    public async Task<CountryModel> GetCountryByName(string countryName)
    {
        return (await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetByName", new { Name = countryName })).FirstOrDefault();
    }

    public async Task<string> InsertCountry(CountryModel country, LogModel logModel)
    {
        ClearCache(CountryCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", country.Id);
        p.Add("CountryName", country.CountryName);
        p.Add("ShortName", country.ShortName);
        p.Add("CountryCode", country.CountryCode);
        p.Add("CurrencyCode", country.CurrencyCode);
        p.Add("MobileNumberDigitCount", country.MobileNumberDigitCount);
        p.Add("MobileNumberCode", country.MobileNumberCode);
        p.Add("SortingPriority", country.SortingPriority);
        p.Add("CreatedBy", country.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Country_Insert", p);
        return country.Id;
    }

    public async Task UpdateCountry(CountryModel country, LogModel logModel)
    {
        ClearCache(CountryCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", country.Id);
        p.Add("CountryName", country.CountryName);
        p.Add("ShortName", country.ShortName);
        p.Add("CountryCode", country.CountryCode);
        p.Add("CurrencyCode", country.CurrencyCode);
        p.Add("MobileNumberDigitCount", country.MobileNumberDigitCount);
        p.Add("MobileNumberCode", country.MobileNumberCode);
        p.Add("SortingPriority", country.SortingPriority);
        p.Add("ModifiedBy", country.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Country_Update", p);
    }

    public async Task DeleteCountry(string countryId, LogModel logModel)
    {
        ClearCache(CountryCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", countryId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Country_Delete", p);
    }

    public async Task<List<CountryModel>> Export()
    {
        return await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DistinctCountryCache:
                _cache.Remove(DistinctCountryCache);
                break;
            case CountryCache:
                var keys = _cache.Get<List<string>>(CountryCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(CountryCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}