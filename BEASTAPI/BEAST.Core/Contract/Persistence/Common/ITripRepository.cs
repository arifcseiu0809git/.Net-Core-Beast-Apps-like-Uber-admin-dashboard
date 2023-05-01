using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface ITripRepository
{
    Task<PaginatedListModel<TripModel>> GetTrips(string statusId, int pageNumber);
    Task<TripModel> GetTripById(string tripId);
    Task<List<TripModel>> Filter(string StatusId, string VehicleTypId, string DriverName, string PassengerName, string ContactNo);
    Task<PaginatedListModel<TripModel>> GetTripsByDriverId(string tripId, int pageNumber);
    Task<string> InsertTrip(TripModel trip, LogModel logModel);
    Task UpdateTrip(TripModel trip, LogModel logModel);
    Task DeleteTrip(string tripId, LogModel logModel);
    Task<List<TripModel>> Export();
}