using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Persistence;

public class VehicleTypeRepository : IVehicleTypeRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string VehicleTypeCache = "VehicleTypeData";
    private const string DistinctVehicleTypeCache = "DistinctVehicleTypeData";
    private const string VehicleTypesWithVehicle = "VehicleTypesWithVehiclesData";

    public VehicleTypeRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<VehicleTypeModel>> GetVehicleTypes(int pageNumber)
    {
        PaginatedListModel<VehicleTypeModel> output = _cache.Get<PaginatedListModel<VehicleTypeModel>>(VehicleTypeCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<VehicleTypeModel, dynamic>("USP_VehicleType_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<VehicleTypeModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(VehicleTypeCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(VehicleTypeCache);
            if (keys is null)
                keys = new List<string> { VehicleTypeCache + pageNumber };
            else
                keys.Add(VehicleTypeCache + pageNumber);
            _cache.Set(VehicleTypeCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
	public async Task<VehicleTypeModel> GetVehicleTypeById(string VehicleTypeId)
    {
        return (await _dataAccessHelper.QueryData<VehicleTypeModel, dynamic>("USP_VehicleType_GetById", new { Id = VehicleTypeId })).SingleOrDefault();
    }

    public async Task<List<VehicleTypeModel>> GetDistinctVehicleTypes()
    {
        var output = _cache.Get<List<VehicleTypeModel>>(DistinctVehicleTypeCache);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<VehicleTypeModel, dynamic>("USP_VehicleType_GetDistinct", new { });
            _cache.Set(DistinctVehicleTypeCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
    public async Task<string> InsertVehicleType(VehicleTypeModel vehicleType, LogModel logModel)
    {
        ClearCache(VehicleTypeCache);
        ClearCache(VehicleTypesWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleType.Id);
        p.Add("Name", vehicleType.Name);
        p.Add("UnitPricePerKm", vehicleType.UnitPricePerKm);
        p.Add("WaitingTimeCostPerMin", vehicleType.WaitingTimeCostPerMin);
        p.Add("ImageName", vehicleType.ImageName);
        p.Add("ImageUrl", vehicleType.ImageUrl);
        p.Add("Descriptions", vehicleType.Descriptions);
        p.Add("SortingPriority", vehicleType.SortingPriority);
        p.Add("IsActive", vehicleType.IsActive);
        p.Add("IsDeleted", vehicleType.IsDeleted);
        p.Add("CreatedBy", vehicleType.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        try
        {
            await _dataAccessHelper.ExecuteData("USP_VehicleType_Insert", p);
            return vehicleType.Id;
        }
        catch (Exception ex)
        { throw; }
    }

    public async Task<string> UpdateVehicleType(VehicleTypeModel vehicleType, LogModel logModel)
    {
        ClearCache(VehicleTypeCache);
        ClearCache(VehicleTypesWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleType.Id);
        p.Add("Name", vehicleType.Name);
        p.Add("UnitPricePerKm", vehicleType.UnitPricePerKm);
        p.Add("WaitingTimeCostPerMin", vehicleType.WaitingTimeCostPerMin);
        p.Add("ImageName", vehicleType.ImageName);
        p.Add("ImageUrl", vehicleType.ImageUrl);
        p.Add("Descriptions", vehicleType.Descriptions);
        p.Add("SortingPriority", vehicleType.SortingPriority);
        p.Add("IsActive", vehicleType.IsActive);
        p.Add("IsDeleted", vehicleType.IsDeleted);
        p.Add("ModifiedBy", vehicleType.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        try
        {
            await _dataAccessHelper.ExecuteData("USP_VehicleType_Update", p);
            return vehicleType.Id;
        }
        catch (Exception ex)
        { throw; }
    }

    public async Task<string> DeleteVehicleType(string vehicleTypeId, LogModel logModel)
    {
        try
        {
            ClearCache(VehicleTypeCache);
            ClearCache(VehicleTypesWithVehicle);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", vehicleTypeId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_VehicleType_Delete", p);
            return vehicleTypeId;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<VehicleTypeModel>> Export()
    {
        return await _dataAccessHelper.QueryData<VehicleTypeModel, dynamic>("USP_VehicleType_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DistinctVehicleTypeCache:
                _cache.Remove(DistinctVehicleTypeCache);
                break;
            case VehicleTypeCache:
                var keys = _cache.Get<List<string>>(VehicleTypeCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(VehicleTypeCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}