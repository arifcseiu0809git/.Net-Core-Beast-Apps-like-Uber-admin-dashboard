using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence.Transaction;

public interface ITransactionResponseRepository
{
    Task<string> InsertTransactionResponse(TransactionResponseModel transactionResponse, LogModel logModel);
    Task UpdateTransactionResponse(TransactionResponseModel transactionResponse, LogModel logModel);
    Task DeleteTransactionResponse(string transactionResponseId, LogModel logModel);
    Task<TransactionResponseModel> GetTransactionResponseById(string transactionResponseId);
    Task<PaginatedListModel<TransactionResponseModel>> GetTransactionResponses(int pageNumber);
    Task<List<TransactionResponseModel>> Export();
}