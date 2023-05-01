using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Core.Contract.Persistence.Map;

public interface ITripInitialRepository
{
    Task<PaginatedListModel<TripInitialModel>> GetTripInitials(int pageNumber);
    Task<List<TripInitialModel>> Filter(string StatusId, string VehicleTypId, string DriverName, string  PassengerName); 
    Task<TripInitialModel> GetTripInitialById(string tripInitialId);
    Task<int> InsertTripInitial(TripInitialModel TripInitial, LogModel logModel);
    Task UpdateTripInitial(TripInitialModel TripInitial, LogModel logModel);
    Task <string> MakePayment(TripInitialModel TripInitial, LogModel logModel);
    Task DeleteTripInitial(string tripInitialId, LogModel logModel);
    Task<List<TripInitialModel>> Export();
}