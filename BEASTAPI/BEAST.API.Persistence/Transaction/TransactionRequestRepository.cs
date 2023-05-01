using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using System.Data;
using BEASTAPI.Core.Contract.Persistence.Transaction;

namespace BEASTAPI.Persistence.Transaction;

public class TransactionRequestRepository : ITransactionRequestRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string TransactionRequestCache = "TransactionRequestData";

    public TransactionRequestRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<PaginatedListModel<TransactionRequestModel>> GetTransactionRequests(int pageNumber)
    {
        PaginatedListModel<TransactionRequestModel> output = _cache.Get<PaginatedListModel<TransactionRequestModel>>(TransactionRequestCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<TransactionRequestModel, dynamic>("USP_TransactionRequest_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<TransactionRequestModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(TransactionRequestCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(TransactionRequestCache);
            if (keys is null)
                keys = new List<string> { TransactionRequestCache + pageNumber };
            else
                keys.Add(TransactionRequestCache + pageNumber);
            _cache.Set(TransactionRequestCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<TransactionRequestModel> GetTransactionRequestById(string smsId)
    {
        return (await _dataAccessHelper.QueryData<TransactionRequestModel, dynamic>("USP_TransactionRequest_GetById", new { Id = smsId })).SingleOrDefault();
    }

    public async Task<string> InsertTransactionRequest(TransactionRequestModel transactionRequest, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionRequestCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionRequest.Id);
            p.Add("TripId", transactionRequest.TripId);
            p.Add("APIEndPointRequestData", transactionRequest.APIEndPointRequestData);
            p.Add("IsActive", transactionRequest.IsActive);
            p.Add("IsDeleted", transactionRequest.IsDeleted);
            p.Add("CreatedBy", transactionRequest.CreatedBy);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionRequest_Insert", p);
            return transactionRequest.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task UpdateTransactionRequest(TransactionRequestModel transactionRequest, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionRequestCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionRequest.Id);
            p.Add("TripId", transactionRequest.TripId);
            p.Add("APIEndPointRequestData", transactionRequest.APIEndPointRequestData);
            p.Add("IsActive", transactionRequest.IsActive);
            p.Add("IsDeleted", transactionRequest.IsDeleted);
            p.Add("ModifiedBy", transactionRequest.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionRequest_Update", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task DeleteTransactionRequest(string transactionRequestId, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionRequestCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionRequestId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionRequest_Delete", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<TransactionRequestModel>> Export()
    {
        return await _dataAccessHelper.QueryData<TransactionRequestModel, dynamic>("USP_TransactionRequest_Export", new { });
    }

    private void ClearCache(string key)
    {
        switch (key)
        {
            case TransactionRequestCache:
                var keys = _cache.Get<List<string>>(TransactionRequestCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(TransactionRequestCache);
                }
                break;
            default:
                break;
        }
    }
}