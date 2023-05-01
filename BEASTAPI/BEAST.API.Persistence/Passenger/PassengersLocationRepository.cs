using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Passenger;

public class PassengersLocationRepository : IPassengersLocationRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string PassengersLocationCache = "PassengersLocationData";

    public PassengersLocationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<PassengersLocationModel>> GetPassengersLocations(int pageNumber)
    {
        PaginatedListModel<PassengersLocationModel> output = _cache.Get<PaginatedListModel<PassengersLocationModel>>(PassengersLocationCache + pageNumber);
        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<PassengersLocationModel, dynamic>("USP_PassengerLocation_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));
            output = new PaginatedListModel<PassengersLocationModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };
            _cache.Set(PassengersLocationCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
            List<string> keys = _cache.Get<List<string>>(PassengersLocationCache);
            if (keys is null)
                keys = new List<string> { PassengersLocationCache + pageNumber };
            else
                keys.Add(PassengersLocationCache + pageNumber);
            _cache.Set(PassengersLocationCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }
        return output;
    }
    public async Task<PassengersLocationModel> GetPassengersLocationById(string passengersLocationId)
    {
        return (await _dataAccessHelper.QueryData<PassengersLocationModel, dynamic>("USP_PassengerLocation_GetById", new { Id = passengersLocationId })).SingleOrDefault();
    }
    public async Task<PassengersLocationModel> GetPassengersLocationByName(string passengersLocationName)
    {
        return (await _dataAccessHelper.QueryData<PassengersLocationModel, dynamic>("USP_PassengersLocation_GetByName", new { Name = passengersLocationName })).SingleOrDefault();
    }
    public async Task<string> InsertPassengersLocation(PassengersLocationModel passengersLocation, LogModel logModel)
    {
        ClearCache(PassengersLocationCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", passengersLocation.Id);
        p.Add("PassengerId", passengersLocation.PassengerId);
        p.Add("LandmarkName", passengersLocation.LandmarkName);
        p.Add("LandmarkType", passengersLocation.LandmarkType);
        p.Add("ZipCode", passengersLocation.ZipCode);
        p.Add("MapLatitude", passengersLocation.MapLatitude);
        p.Add("MapLongitude", passengersLocation.MapLongitude);
        p.Add("LandmarkTypeIcon", passengersLocation.LandmarkTypeIcon);
        p.Add("SortingPriority", passengersLocation.SortingPriority);
        //p.Add("IsActive", passengersLocation.IsActive);
        //p.Add("IsLocked", passengersLocation.IsLocked);
        //p.Add("IsDeleted", passengersLocation.IsDeleted);
        p.Add("CreatedDate", DateTime.Now);
        p.Add("ModifiedDate", DateTime.Now);
        p.Add("CreatedBy", passengersLocation.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PassengerLocation_Insert", p);
        return passengersLocation.Id;
    }
    public async Task UpdatePassengersLocation(PassengersLocationModel passengersLocation, LogModel logModel)
    {
        ClearCache(PassengersLocationCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("PassengerId", passengersLocation.PassengerId);
        p.Add("LandmarkName", passengersLocation.LandmarkName);
        p.Add("LandmarkType", passengersLocation.LandmarkType);
        p.Add("ZipCode", passengersLocation.ZipCode);
        p.Add("MapLatitude", passengersLocation.MapLatitude);
        p.Add("MapLongitude", passengersLocation.MapLongitude);
        p.Add("LandmarkTypeIcon", passengersLocation.LandmarkTypeIcon);
        p.Add("SortingPriority", passengersLocation.SortingPriority);
        p.Add("Id", passengersLocation.Id);
        //p.Add("IsActive", passengersLocation.IsActive);
        //p.Add("IsLocked", passengersLocation.IsLocked);
        //p.Add("IsDeleted", passengersLocation.IsDeleted);
        //p.Add("ModifiedBy", passengersLocation.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);
        await _dataAccessHelper.ExecuteData("USP_PassengerLocation_Update", p);
    }
    public async Task DeletePassengersLocation(string passengersLocationId, LogModel logModel)
    {
        ClearCache(PassengersLocationCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", passengersLocationId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_PassengerLocation_Delete", p);
    }
    public async Task<List<PassengersLocationModel>> Export()
    {
        return await _dataAccessHelper.QueryData<PassengersLocationModel, dynamic>("USP_PassengerLocation_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case PassengersLocationCache:
                var keys = _cache.Get<List<string>>(PassengersLocationCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(PassengersLocationCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}