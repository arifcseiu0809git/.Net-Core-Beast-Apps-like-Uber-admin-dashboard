using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Contract.Persistence;

namespace BEAST.API.Persistence;

public class VehicleRepository : IVehicleRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string VehicleCache = "VehicleData";
    private const string DistinctCategoryCache = "DistinctCategoryData";
    private const string VehiclesWithVehicle = "VehiclesWithVehicleData";

    public VehicleRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<Vehicle>> GetVehicles(int pageNumber)
    {
        PaginatedListModel<Vehicle> output = _cache.Get<PaginatedListModel<Vehicle>>(VehicleCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<Vehicle, dynamic>("USP_Vehicle_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<Vehicle>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(VehicleCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(VehicleCache);
            if (keys is null)
                keys = new List<string> { VehicleCache + pageNumber };
            else
                keys.Add(VehicleCache + pageNumber);
            _cache.Set(VehicleCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
    public async Task<Vehicle> GetVehicleById(string vehicleId)
    {
        return (await _dataAccessHelper.QueryData<Vehicle, dynamic>("USP_Vehicle_GetById", new { Id = vehicleId })).FirstOrDefault();
    }
	public async Task<List<Vehicle>> GetVehicleByTypeId(string vehicleTypeId)
    { 
		return (await _dataAccessHelper.QueryData<Vehicle, dynamic>("USP_Vehicle_GetByVehicleId", new { VehicleTypeId = vehicleTypeId }));
	}

	public async Task<string> InsertVehicle(Vehicle vehicle, LogModel logModel)
    {
        ClearCache(VehicleCache);
        ClearCache(VehiclesWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicle.Id);
        p.Add("VehicleBrandId", vehicle.VehicleBrandId);
        p.Add("VehicleModelId", vehicle.VehicleModelId);
        p.Add("RegistrationNo", vehicle.RegistrationNo);
        p.Add("Color", vehicle.Color);
        p.Add("Fuel", vehicle.Fuel);
        p.Add("Seat", vehicle.Seat);
        p.Add("EngineNo", vehicle.EngineNo);
        p.Add("ChassisNo", vehicle.ChassisNo);
        p.Add("Weight", vehicle.Weight);
        p.Add("Laden", vehicle.Laden);
        p.Add("Authority", vehicle.Authority);
        p.Add("OwnerType", vehicle.OwnerType);
        p.Add("FitnessExpiredOn", vehicle.FitnessExpiredOn);
        p.Add("InsuranceExpiresOn", vehicle.InsuranceExpiresOn);
        p.Add("IsApproved", vehicle.IsApproved);
        p.Add("ApprovedBy", vehicle.ApprovedBy);
        p.Add("ApprovedOn", DateTime.Now);
        p.Add("RegistrationDate", vehicle.RegistrationDate);
        p.Add("VehicleTypeId", vehicle.VehicleTypeId);
        p.Add("StatusId", vehicle.StatusId);
        p.Add("CreatedBy", vehicle.CreatedBy);
        p.Add("IsActive", vehicle.IsActive);
        p.Add("IsDeleted", vehicle.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Vehicle_Insert", p);
        return vehicle.Id;
    }

    public async Task UpdateVehicle(Vehicle vehicle, LogModel logModel)
    {
        ClearCache(VehicleCache);
        ClearCache(VehiclesWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicle.Id);
        p.Add("VehicleBrandId", vehicle.VehicleBrandId);
        p.Add("VehicleModelId", vehicle.VehicleModelId);
        p.Add("RegistrationNo", vehicle.RegistrationNo);
        p.Add("Color", vehicle.Color);
        p.Add("Fuel", vehicle.Fuel);
        p.Add("Seat", vehicle.Seat);
        p.Add("EngineNo", vehicle.EngineNo);
        p.Add("ChassisNo", vehicle.ChassisNo);
        p.Add("Weight", vehicle.Weight);
        p.Add("Laden", vehicle.Laden);
        p.Add("Authority", vehicle.Authority);
        p.Add("OwnerType", vehicle.OwnerType);
        p.Add("FitnessExpiredOn", vehicle.FitnessExpiredOn);
        p.Add("InsuranceExpiresOn", vehicle.InsuranceExpiresOn);
        p.Add("IsApproved", vehicle.IsApproved);
        p.Add("ApprovedBy", vehicle.ApprovedBy);
        p.Add("ApprovedOn", vehicle.ApprovedOn);
        p.Add("RegistrationDate", vehicle.RegistrationDate);
        p.Add("VehicleTypeId", vehicle.VehicleTypeId);
        p.Add("StatusId", vehicle.StatusId);
        p.Add("ModifiedBy", vehicle.ModifiedBy);
        p.Add("IsActive", vehicle.IsActive);
        p.Add("IsDeleted", vehicle.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Vehicle_Update", p);
    }

    public async Task DeleteVehicle(string vehicleId, LogModel logModel)
    {
        ClearCache(VehicleCache);
        ClearCache(VehiclesWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Vehicle_Delete", p);
    }

    public async Task<List<Vehicle>> Export()
    {
        return await _dataAccessHelper.QueryData<Vehicle, dynamic>("USP_Vehicle_Export", new { });
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
            case VehicleCache:
                var keys = _cache.Get<List<string>>(VehicleCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(VehicleCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}