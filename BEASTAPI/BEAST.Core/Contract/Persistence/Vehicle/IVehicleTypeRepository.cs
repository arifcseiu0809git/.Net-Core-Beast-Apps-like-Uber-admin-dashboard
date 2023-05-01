using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Vehicle;

public interface IVehicleTypeRepository
{
    Task<PaginatedListModel<VehicleTypeModel>> GetVehicleTypes(int pageNumber);
	Task<List<VehicleTypeModel>> GetDistinctVehicleTypes();
	Task<VehicleTypeModel> GetVehicleTypeById(string VehicleTypeId);
    Task<string> InsertVehicleType(VehicleTypeModel vehicleType, LogModel logModel);
    Task<string> UpdateVehicleType(VehicleTypeModel vehicleType, LogModel logModel);
    Task<string> DeleteVehicleType(string vehicleTypeId, LogModel logModel);
    Task<List<VehicleTypeModel>> Export();
}