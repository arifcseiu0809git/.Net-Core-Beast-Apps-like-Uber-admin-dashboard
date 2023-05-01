using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface ILocationRepository
{
    Task<PaginatedListModel<LocationModel>> GetLocations(int pageNumber);
    Task<LocationModel> GetLocationById(string locationId);
    Task<LocationModel> GetLocationByName(string locationName);
    Task<string> InsertLocation(LocationModel location, LogModel logModel);
    Task UpdateLocation(LocationModel location, LogModel logModel);
    Task DeleteLocation(string locationId, LogModel logModel);
    Task<List<LocationModel>> Export();
}