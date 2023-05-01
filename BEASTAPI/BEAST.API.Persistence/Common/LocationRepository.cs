using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Common;

public class LocationRepository : ILocationRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string LocationCache = "LocationData";

    public LocationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<LocationModel>> GetLocations(int pageNumber)
    {
        PaginatedListModel<LocationModel> output = _cache.Get<PaginatedListModel<LocationModel>>(LocationCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<LocationModel, dynamic>("USP_Location_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<LocationModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(LocationCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(LocationCache);
            if (keys is null)
                keys = new List<string> { LocationCache + pageNumber };
            else
                keys.Add(LocationCache + pageNumber);
            _cache.Set(LocationCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<LocationModel> GetLocationById(string locationId)
    {
        return (await _dataAccessHelper.QueryData<LocationModel, dynamic>("USP_Location_GetById", new { Id = locationId })).FirstOrDefault();
    }


    public async Task<LocationModel> GetLocationByName(string locationName)
    {
        return (await _dataAccessHelper.QueryData<LocationModel, dynamic>("USP_Location_GetByName", new { Name = locationName })).FirstOrDefault();
    }

    public async Task<string> InsertLocation(LocationModel location, LogModel logModel)
    {
        ClearCache(LocationCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", location.Id);
        p.Add("LandmarkName", location.LandmarkName);
        p.Add("ZipCode", location.ZipCode);
        p.Add("MapLatitude", location.MapLatitude);
        p.Add("MapLongitude", location.MapLongitude);
        p.Add("CreatedBy", location.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Location_Insert", p);
        return location.Id;
    }

    public async Task UpdateLocation(LocationModel location, LogModel logModel)
    {
        ClearCache(LocationCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", location.Id);
        p.Add("LandmarkName", location.LandmarkName);
        p.Add("ZipCode", location.ZipCode);
        p.Add("MapLatitude", location.MapLatitude);
        p.Add("MapLongitude", location.MapLongitude);
        p.Add("LastModifiedBy", location.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Location_Update", p);
    }

    public async Task DeleteLocation(string locationId, LogModel logModel)
    {
        ClearCache(LocationCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", locationId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Location_Delete", p);
    }

    public async Task<List<LocationModel>> Export()
    {
        return await _dataAccessHelper.QueryData<LocationModel, dynamic>("USP_Location_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case LocationCache:
                var keys = _cache.Get<List<string>>(LocationCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(LocationCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}