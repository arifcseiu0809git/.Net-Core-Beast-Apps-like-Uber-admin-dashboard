using BEASTAPI.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface IAddressRepository
    {
        Task<PaginatedListModel<AddressModel>> GetAddress(int pageNumber);
        Task<AddressModel> GetAddressById(string addressId);
        Task<string> InsertAddress(AddressModel address, LogModel logModel);
        Task UpdateAddress(AddressModel address, LogModel logModel);
        Task DeleteAddress(string addressId, LogModel logModel);
        Task<List<AddressModel>> Export();
    }
}
