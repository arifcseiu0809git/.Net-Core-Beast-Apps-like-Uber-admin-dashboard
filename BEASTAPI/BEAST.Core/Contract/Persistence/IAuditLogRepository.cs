using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface IAuditLogRepository
{
    Task<PaginatedListModel<LogModel>> GetAuditLogs(int pageNumber);
    Task<LogModel> GetAuditLogById(string auditLogId);
    Task<string> InsertAuditLog(LogModel logModel);
    Task DeleteAuditLog(string auditLogId);
    Task<List<LogModel>> Export();
}