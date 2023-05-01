using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface ISavedAddressRepository
    {
        Task<PaginatedListModel<SavedAddressModel>> GetSavedAddresses(int pageNumber);
        Task<SavedAddressModel> GetSavedAddressById(string SavedAddressId);
        Task<string> InsertSavedAddress(SavedAddressModel savedAddress, LogModel logModel);
        Task UpdateSavedAddress(SavedAddressModel savedAddress, LogModel logModel);
        Task<List<SavedAddressModel>> GetSavedAddressByName(string AddressName);
        Task DeleteSavedAddress(string savedAddressId, LogModel logModel);
        Task<List<SavedAddressModel>> Export();
    }
}
