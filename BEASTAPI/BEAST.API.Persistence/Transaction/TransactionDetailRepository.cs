using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using System.Data;
using BEASTAPI.Core.Contract.Persistence.Transaction;

namespace BEASTAPI.Persistence.Transaction;

public class TransactionDetailRepository : ITransactionDetailRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string TransactionDetailCache = "TransactionDetailData";

    public TransactionDetailRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<PaginatedListModel<TransactionDetailModel>> GetTransactionDetails(int pageNumber)
    {
        PaginatedListModel<TransactionDetailModel> output = _cache.Get<PaginatedListModel<TransactionDetailModel>>(TransactionDetailCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<TransactionDetailModel, dynamic>("USP_TransactionDetail_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<TransactionDetailModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(TransactionDetailCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(TransactionDetailCache);
            if (keys is null)
                keys = new List<string> { TransactionDetailCache + pageNumber };
            else
                keys.Add(TransactionDetailCache + pageNumber);
            _cache.Set(TransactionDetailCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<TransactionDetailModel> GetTransactionDetailById(string transactionDetailId)
    {
        return (await _dataAccessHelper.QueryData<TransactionDetailModel, dynamic>("USP_TransactionDetail_GetById", new { Id = transactionDetailId })).FirstOrDefault();
    }

    public async Task<string> InsertTransactionDetail(TransactionDetailModel transactionDetail, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionDetailCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionDetail.Id);
            p.Add("TransactionId", transactionDetail.TransactionId);
            p.Add("PaymentMethodId", transactionDetail.PaymentMethodId);
            p.Add("TransactionAmount", transactionDetail.TransactionAmount);
            p.Add("StatusId", transactionDetail.StatusId);
            p.Add("IsActive", transactionDetail.IsActive);
            p.Add("IsDeleted", transactionDetail.IsDeleted);
            p.Add("CreatedBy", transactionDetail.CreatedBy);

            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionDetail_Insert", p);
            return transactionDetail.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task UpdateTransactionDetail(TransactionDetailModel transactionDetail, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionDetailCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionDetail.Id);
            p.Add("TransactionId", transactionDetail.TransactionId);
            p.Add("PaymentMethodId", transactionDetail.PaymentMethodId);
            p.Add("TransactionAmount", transactionDetail.TransactionAmount);
            p.Add("StatusId", transactionDetail.StatusId);
            p.Add("IsActive", transactionDetail.IsActive);
            p.Add("IsDeleted", transactionDetail.IsDeleted);
            p.Add("ModifiedBy", transactionDetail.ModifiedBy);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionDetail_Update", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task DeleteTransactionDetail(string transactionDetailId, LogModel logModel)
    {
        try
        {
            ClearCache(TransactionDetailCache);

            DynamicParameters p = new DynamicParameters();
            p.Add("Id", transactionDetailId);
            p.Add("UserName", logModel.UserName);
            p.Add("UserRole", logModel.UserRole);
            p.Add("IP", logModel.IP);

            await _dataAccessHelper.ExecuteData("USP_TransactionDetail_Delete", p);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<TransactionDetailModel>> Export()
    {
        return await _dataAccessHelper.QueryData<TransactionDetailModel, dynamic>("USP_TransactionDetail_Export", new { });
    }

    private void ClearCache(string key)
    {
        switch (key)
        {
            case TransactionDetailCache:
                var keys = _cache.Get<List<string>>(TransactionDetailCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(TransactionDetailCache);
                }
                break;
            default:
                break;
        }
    }
}