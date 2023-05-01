using Dapper;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using System.Data;

namespace BEASTAPI.Persistence;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;

    public AuditLogRepository(IDataAccessHelper dataAccessHelper, IConfiguration config)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
    }

    public async Task<PaginatedListModel<LogModel>> GetAuditLogs(int pageNumber)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("PageNumber", pageNumber);
        p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
        p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

        var result = await _dataAccessHelper.QueryData<LogModel, dynamic>("USP_AuditLog_GetAll", p);
        int TotalRecords = p.Get<int>("TotalRecords");
        int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

        PaginatedListModel<LogModel> output = new PaginatedListModel<LogModel>
        {
            PageIndex = pageNumber,
            TotalRecords = TotalRecords,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages,
            Items = result.ToList()
        };

        return output;
    }

    public async Task<LogModel> GetAuditLogById(string auditLogId)
    {
        return (await _dataAccessHelper.QueryData<LogModel, dynamic>("USP_AuditLog_GetById", new { Id = auditLogId })).FirstOrDefault();
    }

    public async Task<string> InsertAuditLog(LogModel logModel)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("Id", logModel.Id);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);
        p.Add("TableName", logModel.TableName);
        p.Add("OldData", logModel.OldData);
        p.Add("NewData", logModel.NewData);

        await _dataAccessHelper.ExecuteData("USP_AuditLog_InsertManually", p);
        return logModel.Id;
    }

    public async Task DeleteAuditLog(string auditLogId)
    {
        await _dataAccessHelper.ExecuteData("USP_AuditLog_Delete", new { Id = auditLogId });
    }

    public async Task<List<LogModel>> Export()
    {
        return await _dataAccessHelper.QueryData<LogModel, dynamic>("USP_AuditLog_Export", new { });
    }
}