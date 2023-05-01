using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface ISystemStatusRepository
{
    Task<PaginatedListModel<SystemStatusModel>> GetSystemStatus(int pageNumber);
	Task<List<SystemStatusModel>> GetDistinctSystemStatus();
	Task<SystemStatusModel> GetSystemStatusById(string systemStatusId);
    Task<SystemStatusModel> GetSystemStatusByName(string systemStatusName);
    Task<string> InsertSystemStatus(SystemStatusModel systemStatus, LogModel logModel);
    Task UpdateSystemStatus(SystemStatusModel systemStatus, LogModel logModel);
    Task DeleteSystemStatus(string systemStatusId, LogModel logModel);
    Task<List<SystemStatusModel>> Export();
}