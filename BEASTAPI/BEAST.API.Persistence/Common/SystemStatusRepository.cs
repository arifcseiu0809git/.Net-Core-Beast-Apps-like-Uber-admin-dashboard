using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Persistence.Common;

public class SystemStatusRepository : ISystemStatusRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string SystemStatusCache = "SystemStatusData";

    public SystemStatusRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<SystemStatusModel>> GetSystemStatus(int pageNumber)
    {
        PaginatedListModel<SystemStatusModel> output = _cache.Get<PaginatedListModel<SystemStatusModel>>(SystemStatusCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<SystemStatusModel, dynamic>("USP_SystemStatus_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<SystemStatusModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(SystemStatusCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(SystemStatusCache);
            if (keys is null)
                keys = new List<string> { SystemStatusCache + pageNumber };
            else
                keys.Add(SystemStatusCache + pageNumber);
            _cache.Set(SystemStatusCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }
	public async Task<List<SystemStatusModel>> GetDistinctSystemStatus()
	{
        return  await _dataAccessHelper.QueryData<SystemStatusModel, dynamic>("USP_SystemStatus_GetDistinct", new { });
		
	}
	public async Task<SystemStatusModel> GetSystemStatusById(string systemStatusId)
    {
        return (await _dataAccessHelper.QueryData<SystemStatusModel, dynamic>("USP_SystemStatus_GetById", new { Id = systemStatusId })).FirstOrDefault();
    }
   
    public async Task<SystemStatusModel> GetSystemStatusByName(string systemStatusName)
    {
        return (await _dataAccessHelper.QueryData<SystemStatusModel, dynamic>("USP_SystemStatus_GetByName", new { Name = systemStatusName })).FirstOrDefault();
    }

    public async Task<string> InsertSystemStatus(SystemStatusModel systemStatus, LogModel logModel)
    {
        ClearCache(SystemStatusCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", systemStatus.Id);
        p.Add("Name", systemStatus.Name);
        p.Add("CreatedBy", systemStatus.CreatedBy);

        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SystemStatus_Insert", p);
        return systemStatus.Id;
    }

    public async Task UpdateSystemStatus(SystemStatusModel systemStatus, LogModel logModel)
    {
        ClearCache(SystemStatusCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", systemStatus.Id);
        p.Add("Name", systemStatus.Name);
        p.Add("ModifiedBy", systemStatus.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SystemStatus_Update", p);
    }

    public async Task DeleteSystemStatus(string systemStatusId, LogModel logModel)
    {
        ClearCache(SystemStatusCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", systemStatusId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_SystemStatus_Delete", p);
    }

    public async Task<List<SystemStatusModel>> Export()
    {
        return await _dataAccessHelper.QueryData<SystemStatusModel, dynamic>("USP_SystemStatus_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case SystemStatusCache:
                var keys = _cache.Get<List<string>>(SystemStatusCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(SystemStatusCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}