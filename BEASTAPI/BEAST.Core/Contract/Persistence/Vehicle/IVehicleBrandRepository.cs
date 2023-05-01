using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Vehicle;

public interface IVehicleBrandRepository
{
    Task<PaginatedListModel<VehicleBrandModel>> GetVehicleBrands(int pageNumber);
    Task<List<VehicleBrandModel>> GetDistinctVehicleBrands();

	Task<VehicleBrandModel> GetVehicleBrandById(string vehicleBrandId);
    Task<string> InsertVehicleBrand(VehicleBrandModel vehicleBrand, LogModel logModel);
    Task UpdateVehicleBrand(VehicleBrandModel vehicleBrand, LogModel logModel);
    Task DeleteVehicleBrand(string vehicleBrandId, LogModel logModel);
    Task<List<VehicleBrandModel>> Export();
}