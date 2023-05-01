using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using System.Data;
using BEASTAPI.Core.Contract.Persistence.Transaction;

namespace BEASTAPI.Persistence.Transaction;

public class TransactionResponseRepository : ITransactionResponseRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string TransactionResponseCache = "TransactionResponseData";

    public TransactionResponseRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<PaginatedListModel<TransactionResponseModel>> GetTransactionResponses(int pageNumber)
    {
        PaginatedListModel<TransactionResponseModel> output = _cache.Get<PaginatedListModel<TransactionResponseModel>>(TransactionResponseCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<TransactionResponseModel, dynamic>("USP_TransactionResponse_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<TransactionResponseModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(TransactionResponseCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(TransactionResponseCache);
            if (keys is null)
                keys = new List<string> { TransactionResponseCache + pageNumber };
            else
                keys.Add(TransactionResponseCache + pageNumber);
            _cache.Set(TransactionResponseCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<TransactionResponseModel> GetTransactionResponseById(string transactionResponseId)
    {
        return (await _dataAccessHelper.QueryData<TransactionResponseModel, dynamic>("USP_TransactionResponse_GetById", new { Id = transactionResponseId })).SingleOrDefault();
    }

    public async Task<string> InsertTransactionResponse(TransactionResponseModel transactionResponse, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionResponseCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionResponse.Id);
            p.Add("TripId", transactionResponse.TripId);
            p.Add("APIResponseData", transactionResponse.APIResponseData);
            p.Add("IsActive", transactionResponse.IsActive);
            p.Add("IsDeleted", transactionResponse.IsDeleted);
            p.Add("CreatedBy", transactionResponse.CreatedBy);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionResponse_Insert", p);
            return transactionResponse.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task UpdateTransactionResponse(TransactionResponseModel transactionResponse, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionResponseCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionResponse.Id);
            p.Add("TripId", transactionResponse.TripId);
            p.Add("APIResponseData", transactionResponse.APIResponseData);
            p.Add("IsActive", transactionResponse.IsActive);
            p.Add("IsDeleted", transactionResponse.IsDeleted);
            p.Add("ModifiedBy", transactionResponse.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionResponse_Update", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task DeleteTransactionResponse(string smsId, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionResponseCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", smsId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionResponse_Delete", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<TransactionResponseModel>> Export()
    {
        return await _dataAccessHelper.QueryData<TransactionResponseModel, dynamic>("USP_TransactionResponse_Export", new { });
    }

    private void ClearCache(string key)
    {
        switch (key)
        {
            case TransactionResponseCache:
                var keys = _cache.Get<List<string>>(TransactionResponseCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(TransactionResponseCache);
                }
                break;
            default:
                break;
        }
    }
}