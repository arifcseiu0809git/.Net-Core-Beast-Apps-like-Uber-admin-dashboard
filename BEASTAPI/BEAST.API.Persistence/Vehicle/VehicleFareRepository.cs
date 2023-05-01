using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;
using BEASTAPI.Core.Contract.Vehicle;
using BEASTAPI.Core.Contract.Persistence;

namespace BEAST.API.Persistence;

public class VehicleFareRepository : IVehicleFareRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string VehicleFareCache = "VehicleFareData";
    private const string DistinctCategoryCache = "DistinctCategoryData";
    private const string VehicleFareWithVehicle = "VehicleFareWithVehicleTypeData";

    public VehicleFareRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<VehicleFareModel>> GetVehicleFares(int pageNumber)
    {
        PaginatedListModel<VehicleFareModel> output = _cache.Get<PaginatedListModel<VehicleFareModel>>(VehicleFareCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<VehicleFareModel, dynamic>("USP_VehicleFare_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<VehicleFareModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(VehicleFareCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(VehicleFareCache);
            if (keys is null)
                keys = new List<string> { VehicleFareCache + pageNumber };
            else
                keys.Add(VehicleFareCache + pageNumber);
            _cache.Set(VehicleFareCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
    public async Task<VehicleFareModel> GetVehicleFareById(string vehicleFareId)
    {
        return (await _dataAccessHelper.QueryData<VehicleFareModel, dynamic>("USP_VehicleFare_GetById", new { Id = vehicleFareId })).FirstOrDefault();
    }

    public async Task<string> InsertVehicleFare(VehicleFareModel vehicleFare, LogModel logModel)
    {
        ClearCache(VehicleFareCache);
        ClearCache(VehicleFareWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleFare.Id);
        p.Add("VehicleTypeId", vehicleFare.VehicleTypeId);
        p.Add("BaseFare", vehicleFare.BaseFare);
        p.Add("CostPerMinInServiceArea", vehicleFare.CostPerMinInServiceArea);
        p.Add("WaitingCostPerMinInServiceArea", vehicleFare.WaitingCostPerMinInServiceArea);
        p.Add("CostPerKmInServiceArea", vehicleFare.CostPerKmInServiceArea);
        p.Add("BookingFee", vehicleFare.BookingFee);
        p.Add("CostPerMinOutsideServiceArea", vehicleFare.CostPerMinOutsideServiceArea);
        p.Add("WaitingCostPerMinOutsideServiceArea", vehicleFare.WaitingCostPerMinOutsideServiceArea);
        p.Add("CostPerKmOutsideServiceArea", vehicleFare.CostPerKmOutsideServiceArea);
        p.Add("CancelFee", vehicleFare.CancelFee);
        p.Add("CreatedBy", vehicleFare.CreatedBy);
        p.Add("IsActive", vehicleFare.IsActive);
        p.Add("IsDeleted", vehicleFare.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleFare_Insert", p);
        return vehicleFare.Id;
    }

    public async Task UpdateVehicleFare(VehicleFareModel vehicleFare, LogModel logModel)
    {
        ClearCache(VehicleFareCache);
        ClearCache(VehicleFareWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleFare.Id);
        p.Add("VehicleTypeId", vehicleFare.VehicleTypeId);
        p.Add("BaseFare", vehicleFare.BaseFare);
        p.Add("CostPerMinInServiceArea", vehicleFare.CostPerMinInServiceArea);
        p.Add("WaitingCostPerMinInServiceArea", vehicleFare.WaitingCostPerMinInServiceArea);
        p.Add("CostPerKmInServiceArea", vehicleFare.CostPerKmInServiceArea);
        p.Add("BookingFee", vehicleFare.BookingFee);
        p.Add("CostPerMinOutsideServiceArea", vehicleFare.CostPerMinOutsideServiceArea);
        p.Add("WaitingCostPerMinOutsideServiceArea", vehicleFare.WaitingCostPerMinOutsideServiceArea);
        p.Add("CostPerKmOutsideServiceArea", vehicleFare.CostPerKmOutsideServiceArea);
        p.Add("CancelFee", vehicleFare.CancelFee);
        p.Add("ModifiedBy", vehicleFare.ModifiedBy);
        p.Add("IsActive", vehicleFare.IsActive);
        p.Add("IsDeleted", vehicleFare.IsDeleted);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleFare_Update", p);
    }

    public async Task DeleteVehicleFare(string vehicleFareId, LogModel logModel)
    {
        ClearCache(VehicleFareCache);
        ClearCache(VehicleFareWithVehicle);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", vehicleFareId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_VehicleFare_Delete", p);
    }

    public async Task<List<VehicleFareModel>> Export()
    {
        return await _dataAccessHelper.QueryData<VehicleFareModel, dynamic>("USP_VehicleFare_Export", new { });
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
            case VehicleFareCache:
                var keys = _cache.Get<List<string>>(VehicleFareCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(VehicleFareCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}