using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Vehicle;

public interface IVehicleRepository
{
    Task<PaginatedListModel<BEASTAPI.Core.Model.Vehicle.Vehicle>> GetVehicles(int pageNumber);
    Task<BEASTAPI.Core.Model.Vehicle.Vehicle> GetVehicleById(string vehicleId);
	Task<List<BEASTAPI.Core.Model.Vehicle.Vehicle>> GetVehicleByTypeId(string vehicleTypeId);
	Task<string> InsertVehicle(BEASTAPI.Core.Model.Vehicle.Vehicle vehicle, LogModel logModel);
    Task UpdateVehicle(BEASTAPI.Core.Model.Vehicle.Vehicle vehicle, LogModel logModel);
    Task DeleteVehicle(string vehicleId, LogModel logModel);
    Task<List<BEASTAPI.Core.Model.Vehicle.Vehicle>> Export();
}