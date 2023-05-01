using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence;

public class CityRepository : ICityRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string CityCache = "CityData";
    private const string DistinctCityCache = "DistinctCityData";
    //private const string CitiesWithPiesCache = "CategoriesWithPiesData";

    public CityRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<CityModel>> GetCities(int pageNumber)
    {
        PaginatedListModel<CityModel> output = _cache.Get<PaginatedListModel<CityModel>>(CityCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<CityModel, dynamic>("USP_City_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<CityModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(CityCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(CityCache);
            if (keys is null)
                keys = new List<string> { CityCache + pageNumber };
            else
                keys.Add(CityCache + pageNumber);
            _cache.Set(CityCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<List<CityModel>> GetDistinctCities()
    {
        var output = _cache.Get<List<CityModel>>(DistinctCityCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<CityModel, dynamic>("USP_City_GetDistinct", new { });
            _cache.Set(DistinctCityCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<List<CityModel>> GetCityByCuntryId(string Id) //country id
    {
        return (await _dataAccessHelper.QueryData<CityModel, dynamic>("GetCityByCountryId", new { Id = Id })).ToList();
    }
    public async Task<CityModel> GetCityById(string cityId)
    {
        return (await _dataAccessHelper.QueryData<CityModel, dynamic>("USP_City_GetById", new { Id = cityId })).FirstOrDefault();
    }

    public async Task<CityModel> GetCityByName(string cityName)
    {
        return (await _dataAccessHelper.QueryData<CityModel, dynamic>("USP_City_GetByName", new { Name = cityName })).FirstOrDefault();
    }

    public async Task<string> InsertCity(CityModel city, LogModel logModel)
    {
        ClearCache(CityCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", city.Id);
        p.Add("Name", city.Name);
        p.Add("CountryId", city.CountryId);
        p.Add("CreatedBy", city.CreatedBy);
        p.Add("IsActive", city.IsActive);
        p.Add("IsDeleted", city.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_City_Insert", p);
        return city.Id;
    }

    public async Task UpdateCity(CityModel city, LogModel logModel)
    {
        ClearCache(CityCache);
        //ClearCache(CategoriesWithPiesCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", city.Id);
        p.Add("Name", city.Name);
        p.Add("CountryId", city.CountryId);
        p.Add("IsActive", city.IsActive);
        p.Add("IsDeleted", city.IsDeleted);
        p.Add("ModifiedBy", city.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_City_Update", p);
    }

    public async Task DeleteCity(string cityId, LogModel logModel)
    {
        ClearCache(CityCache);
        //ClearCache(CategoriesWithPiesCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", cityId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_City_Delete", p);
    }

    public async Task<List<CityModel>> Export()
    {
        return await _dataAccessHelper.QueryData<CityModel, dynamic>("USP_City_Export", new { });
    }
    #endregion

    //#region "Customized Methods"
    //public async Task<List<CategoryModel>> GetCategoriesWithPies()
    //{
    //    var output = _cache.Get<List<CategoryModel>>(CategoriesWithPiesCache);

    //    if (output is null)
    //    {
    //        using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
    //        {
    //            DynamicParameters p = new DynamicParameters();
    //            Dictionary<string, CategoryModel> dictionary = new Dictionary<string, CategoryModel>();

    //            var categories = await connection.QueryAsync<CategoryModel, PieModel, CategoryModel>("USP_Category_GetWithPies", (category, pie) =>
    //            {
    //                CategoryModel currentCategory;
    //                if (!dictionary.TryGetValue(category.Id, out currentCategory))
    //                {
    //                    currentCategory = category;
    //                    dictionary.Add(currentCategory.Id, currentCategory);
    //                }

    //                currentCategory.Pies.Add(pie);
    //                return currentCategory;
    //            }, p, commandType: CommandType.StoredProcedure);

    //            output = categories.Distinct().ToList();
    //            _cache.Set(CategoriesWithPiesCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
    //        }
    //    }

    //    return output;
    //}
    //#endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DistinctCityCache:
                _cache.Remove(DistinctCityCache);
                break;
            case CityCache:
                var keys = _cache.Get<List<string>>(CityCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(CityCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}