using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface IDocumentTypeRepository
{
    Task<PaginatedListModel<DocumentTypeModel>> GetDocumentTypes(int pageNumber);
    Task<DocumentTypeModel> GetDocumentTypeById(string DocumentTypeId);

	Task<string> InsertDocumentType(DocumentTypeModel DocumentType, LogModel logModel);
    Task UpdateDocumentType(DocumentTypeModel DocumentType, LogModel logModel);
    Task DeleteDocumentType(string DocumentTypeId, LogModel logModel);
    
}