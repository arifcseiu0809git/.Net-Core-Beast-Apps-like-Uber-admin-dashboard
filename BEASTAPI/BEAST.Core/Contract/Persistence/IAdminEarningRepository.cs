using BEASTAPI.Core.Model;
using BEASTAPI.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface IAdminEarningRepository
    {
        Task<PaginatedListModel<AdminEarning>> GetEarning(int pageNumber);
        Task<AdminEarning> GetEarningByTripId(string tripId);
        Task<PaginatedListModel<AdminEarning>> GetEarningByDriverId(string driverId, int pageNumber);
        Task<bool> InsertDriverCommissionsByDateRange(AdminEarningInsertViewModel model, LogModel logModel);
        Task UpdateEarning(AdminEarning adminEarning, LogModel logModel);
        Task DeleteEarning(string id, LogModel logModel);
        Task<List<AdminEarning>> Export(string driverId = null);
        Task<decimal> GetDueCommissionByDriverId(string driverId, DateTime fromDate, DateTime toDate);
    }
}
