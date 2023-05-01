using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Contract.Persistence.Passenger;

public interface IPassengersLocationRepository
{
    Task<PaginatedListModel<PassengersLocationModel>> GetPassengersLocations(int pageNumber);
    Task<PassengersLocationModel> GetPassengersLocationById(string passengersLocationId);
    Task<PassengersLocationModel> GetPassengersLocationByName(string passengersLocationName);
    Task<string> InsertPassengersLocation(PassengersLocationModel passengersLocation, LogModel logModel);
    Task UpdatePassengersLocation(PassengersLocationModel passengersLocation, LogModel logModel);
    Task DeletePassengersLocation(string passengersLocationId, LogModel logModel);
    Task<List<PassengersLocationModel>> Export();
}