using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Vehicle;

public interface IVehicleFareRepository
{
    Task<PaginatedListModel<VehicleFareModel>> GetVehicleFares(int pageNumber);
    Task<VehicleFareModel> GetVehicleFareById(string vehicleFareId);
    Task<string> InsertVehicleFare(VehicleFareModel vehicleFare, LogModel logModel);
    Task UpdateVehicleFare(VehicleFareModel vehicleFare, LogModel logModel);
    Task DeleteVehicleFare(string vehicleFareId, LogModel logModel);
    Task<List<VehicleFareModel>> Export();
}