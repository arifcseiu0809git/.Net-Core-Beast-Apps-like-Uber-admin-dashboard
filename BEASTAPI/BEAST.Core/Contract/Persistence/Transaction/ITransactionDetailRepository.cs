using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence.Transaction;

public interface ITransactionDetailRepository
{
    Task<string> InsertTransactionDetail(TransactionDetailModel transactionDetail, LogModel logModel);
    Task UpdateTransactionDetail(TransactionDetailModel transactionDetail, LogModel logModel);
    Task DeleteTransactionDetail(string transactionDetailId, LogModel logModel);
    Task<TransactionDetailModel> GetTransactionDetailById(string transactionDetailId);
    Task<PaginatedListModel<TransactionDetailModel>> GetTransactionDetails(int pageNumber);
    Task<List<TransactionDetailModel>> Export();
}