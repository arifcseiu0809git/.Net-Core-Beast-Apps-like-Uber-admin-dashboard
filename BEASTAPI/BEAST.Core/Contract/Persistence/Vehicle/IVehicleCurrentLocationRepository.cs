using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Vehicle;

public interface IVehicleCurrentLocationRepository
{
    Task<PaginatedListModel<VehicleCurrentLocationModel>> GetVehicleCurrentLocations(int pageNumber);
    Task<VehicleCurrentLocationModel> GetVehicleCurrentLocationById(string vehicleCurrentLocationId);
    Task<VehicleCurrentLocationModel> GetVehicleCurrentLocationModelById(string vehicleCurrentLocationId);

	Task<string> InsertVehicleCurrentLocation(VehicleCurrentLocationModel vehicleCurrentLocation, LogModel logModel);
    Task UpdateVehicleCurrentLocation(VehicleCurrentLocationModel vehicleCurrentLocation, LogModel logModel);
    Task DeleteVehicleCurrentLocation(string vehicleCurrentLocationId, LogModel logModel);
    Task<List<VehicleCurrentLocationModel>> Export();
}