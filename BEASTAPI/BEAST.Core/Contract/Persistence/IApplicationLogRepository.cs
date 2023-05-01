using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface IApplicationLogRepository
{
    Task<PaginatedListModel<ApplicationLogModel>> GetApplicationLogs(int pageNumber);
    Task<ApplicationLogModel> GetApplicationLogById(string applicationLogId);
    Task DeleteApplicationLog(string applicationLogId);
    Task<List<ApplicationLogModel>> Export();
}