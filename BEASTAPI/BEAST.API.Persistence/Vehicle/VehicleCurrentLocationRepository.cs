using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Contract.Persistence;

namespace BEAST.API.Persistence;

public class VehicleCurrentLocationRepository : IVehicleCurrentLocationRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string VehicleCurrentLocationCache = "VehicleCurrentLocationData";
    private const string DistinctCategoryCache = "DistinctCategoryData";
    private const string VehicleCurrentLocationWithVehicle = "VehicleCurrentLocationWithVehicleData";

    public VehicleCurrentLocationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<VehicleCurrentLocationModel>> GetVehicleCurrentLocations(int pageNumber)
    {
        PaginatedListModel<VehicleCurrentLocationModel> output = _cache.Get<PaginatedListModel<VehicleCurrentLocationModel>>(VehicleCurrentLocationCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<VehicleCurrentLocationModel, dynamic>("USP_VehicleCurrentLocation_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<VehicleCurrentLocationModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(VehicleCurrentLocationCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(VehicleCurrentLocationCache);
            if (keys is null)
                keys = new List<string> { VehicleCurrentLocationCache + pageNumber };
            else
                keys.Add(VehicleCurrentLocationCache + pageNumber);
            _cache.Set(VehicleCurrentLocationCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
    public async Task<VehicleCurrentLocationModel> GetVehicleCurrentLocationById(string vehicleCurrentLocationId)
    {
        return (await _dataAccessHelper.QueryData<VehicleCurrentLocationModel, dynamic>("USP_VehicleCurrentLocation_GetById", new { Id = vehicleCurrentLocationId })).FirstOrDefault();
    }
	public async Task<VehicleCurrentLocationModel> GetVehicleCurrentLocationModelById(string vehicleCurrentLocationId)
	{
		return (await _dataAccessHelper.QueryData<VehicleCurrentLocationModel, dynamic>("USP_VehicleCurrentLocationDistanctModelByBrand_GetById", new { Id = vehicleCurrentLocationId })).FirstOrDefault();
	}
	
	public async Task<string> InsertVehicleCurrentLocation(VehicleCurrentLocationModel vehicleCurrentLocation, LogModel logModel)
    {
        ClearCache(VehicleCurrentLocationCache);
        ClearCache(VehicleCurrentLocationWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleCurrentLocation.Id);
        p.Add("VehicleId", "d39c8ab7-4dd7-4220-b3c1-80fd1c2602e1" /*vehicleCurrentLocation.VehicleId*/);
        p.Add("Latitude", vehicleCurrentLocation.Latitude);
        p.Add("Longitude", vehicleCurrentLocation.Longitude);
        p.Add("PreviousLatitude", vehicleCurrentLocation.PreviousLatitude);
        p.Add("PreviousLongitude", vehicleCurrentLocation.PreviousLongitude);
        p.Add("GoingDirection", vehicleCurrentLocation.GoingDirection);
        p.Add("GoingDegree", vehicleCurrentLocation.GoingDegree);
        p.Add("LastUpdateAt", vehicleCurrentLocation.LastUpdateAt);
        p.Add("IsOnline", vehicleCurrentLocation.IsOnline);
        p.Add("CreatedBy", vehicleCurrentLocation.CreatedBy);
        p.Add("IsActive", vehicleCurrentLocation.IsActive);
        p.Add("IsDeleted", vehicleCurrentLocation.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleCurrentLocation_Insert", p);
        return vehicleCurrentLocation.Id;
    }

    public async Task UpdateVehicleCurrentLocation(VehicleCurrentLocationModel vehicleCurrentLocation, LogModel logModel)
    {
        ClearCache(VehicleCurrentLocationCache);
        ClearCache(VehicleCurrentLocationWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleCurrentLocation.Id);
        p.Add("VehicleId", "d39c8ab7-4dd7-4220-b3c1-80fd1c2602e1" /*vehicleCurrentLocation.VehicleId*/);
		p.Add("Latitude", vehicleCurrentLocation.Latitude);
        p.Add("Longitude", vehicleCurrentLocation.Longitude);
        p.Add("PreviousLatitude", vehicleCurrentLocation.PreviousLatitude);
        p.Add("PreviousLongitude", vehicleCurrentLocation.PreviousLongitude);
        p.Add("GoingDirection", vehicleCurrentLocation.GoingDirection);
        p.Add("GoingDegree", vehicleCurrentLocation.GoingDegree);
        p.Add("LastUpdateAt", vehicleCurrentLocation.LastUpdateAt);
        p.Add("IsOnline", vehicleCurrentLocation.IsOnline);
        p.Add("ModifiedBy", vehicleCurrentLocation.ModifiedBy);
        p.Add("IsActive", vehicleCurrentLocation.IsActive);
        p.Add("IsDeleted", vehicleCurrentLocation.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleCurrentLocation_Update", p);
    }

    public async Task DeleteVehicleCurrentLocation(string vehicleCurrentLocationId, LogModel logModel)
    {
        ClearCache(VehicleCurrentLocationCache);
        ClearCache(VehicleCurrentLocationWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleCurrentLocationId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleCurrentLocation_Delete", p);
    }

    public async Task<List<VehicleCurrentLocationModel>> Export()
    {
        return await _dataAccessHelper.QueryData<VehicleCurrentLocationModel, dynamic>("USP_VehicleCurrentLocation_Export", new { });
    }
    #endregion

    #region "Customized Methods"
   
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            //case CategoriesWithPiesCache:
            //    _cache.Remove(CategoriesWithPiesCache);
            //    break;
            case VehicleCurrentLocationCache:
                var keys = _cache.Get<List<string>>(VehicleCurrentLocationCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(VehicleCurrentLocationCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}