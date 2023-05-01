using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence.Transaction;

public interface ITransactionRequestRepository
{
    Task<string> InsertTransactionRequest(TransactionRequestModel transactionRequest, LogModel logModel);
    Task UpdateTransactionRequest(TransactionRequestModel transactionRequest, LogModel logModel);
    Task DeleteTransactionRequest(string transactionRequestId, LogModel logModel);
    Task<TransactionRequestModel> GetTransactionRequestById(string transactionRequestId);
    Task<PaginatedListModel<TransactionRequestModel>> GetTransactionRequests(int pageNumber);
    Task<List<TransactionRequestModel>> Export();
}