using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface IDocumentRepository
{
    Task<PaginatedListModel<DocumentModel>> GetDocuments(int pageNumber);
    Task<DocumentModel> GetDocumentById(string documentId);
    Task<List<DocumentModel>> GetDocumentsByUserId(string userId);
    Task<string> InsertDocument(DocumentModel document, LogModel logModel);
    Task UpdateDocument(DocumentModel document, LogModel logModel);
    Task DeleteDocument(string documentId, LogModel logModel);
    Task<List<DocumentModel>> Export();
}