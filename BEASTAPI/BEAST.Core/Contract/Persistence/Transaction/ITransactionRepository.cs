using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence.Transaction;

public interface ITransactionRepository
{
    Task<string> InsertTransaction(TransactionModel transaction, LogModel logModel);
    Task UpdateTransaction(TransactionModel transaction, LogModel logModel);
    Task DeleteTransaction(string transactionId, LogModel logModel);
    Task<TransactionModel> GetTransactionById(string transactionId);
    Task<PaginatedListModel<TransactionModel>> GetTransactions(int pageNumber);
    Task<List<TransactionModel>> Export();
}