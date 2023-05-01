using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using System.Data;
using BEASTAPI.Core.Contract.Persistence.Transaction;

namespace BEASTAPI.Persistence.Transaction;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string TransactionCache = "TransactionData";

    public TransactionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<PaginatedListModel<TransactionModel>> GetTransactions(int pageNumber)
    {
        PaginatedListModel<TransactionModel> output = _cache.Get<PaginatedListModel<TransactionModel>>(TransactionCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<TransactionModel, dynamic>("USP_Transaction_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<TransactionModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(TransactionCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(TransactionCache);
            if (keys is null)
                keys = new List<string> { TransactionCache + pageNumber };
            else
                keys.Add(TransactionCache + pageNumber);
            _cache.Set(TransactionCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<TransactionModel> GetTransactionById(string smsId)
    {
        return (await _dataAccessHelper.QueryData<TransactionModel, dynamic>("USP_Transaction_GetById", new { Id = smsId })).SingleOrDefault();
    }

    public async Task<string> InsertTransaction(TransactionModel transaction, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transaction.Id);
            p.Add("TripId", transaction.TripId);
            p.Add("TotalBillAmount", transaction.TotalBillAmount);
            p.Add("BillDate", transaction.BillDate);
            p.Add("IsActive", transaction.IsActive);
            p.Add("IsDeleted", transaction.IsDeleted);
            p.Add("CreatedBy", transaction.CreatedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_Transaction_Insert", p);
            return transaction.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task UpdateTransaction(TransactionModel transaction, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transaction.Id);
            p.Add("TripId", transaction.TripId);
            p.Add("TotalBillAmount", transaction.TotalBillAmount);
            p.Add("BillDate", transaction.BillDate);
            p.Add("IsActive", transaction.IsActive);
            p.Add("IsDeleted", transaction.IsDeleted);
            p.Add("ModifiedBy", transaction.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_Transaction_Update", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task DeleteTransaction(string transactionId, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_Transaction_Delete", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<TransactionModel>> Export()
    {
        return await _dataAccessHelper.QueryData<TransactionModel, dynamic>("USP_Transaction_Export", new { });
    }

    private void ClearCache(string key)
    {
        switch (key)
        {
            case TransactionCache:
                var keys = _cache.Get<List<string>>(TransactionCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(TransactionCache);
                }
                break;
            default:
                break;
        }
    }
}