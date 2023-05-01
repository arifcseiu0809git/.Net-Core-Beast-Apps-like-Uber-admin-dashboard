using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Vehicle;

public interface IVehicleModelRepository
{
    Task<PaginatedListModel<VehicleModel>> GetVehicleModels(int pageNumber);
	Task<List<VehicleModel>> GetDistinctVehicleModels();
	Task<VehicleModel> GetVehicleModelById(string vehicleModelId);
    Task<string> InsertVehicleModel(VehicleModel vehicleModel, LogModel logModel);
    Task UpdateVehicleModel(VehicleModel vehicleModel, LogModel logModel);
    Task DeleteVehicleModel(string vehicleModelId, LogModel logModel);
    Task<List<VehicleModel>> Export();
}