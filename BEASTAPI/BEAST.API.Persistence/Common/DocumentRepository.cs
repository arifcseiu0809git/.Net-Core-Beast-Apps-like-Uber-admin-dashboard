using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Common;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string DocumentCache = "DocumentData";

    public DocumentRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<DocumentModel>> GetDocuments(int pageNumber)
    {
        PaginatedListModel<DocumentModel> output = _cache.Get<PaginatedListModel<DocumentModel>>(DocumentCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<DocumentModel, dynamic>("USP_Document_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<DocumentModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(DocumentCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(DocumentCache);
            if (keys is null)
                keys = new List<string> { DocumentCache + pageNumber };
            else
                keys.Add(DocumentCache + pageNumber);
            _cache.Set(DocumentCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<DocumentModel> GetDocumentById(string documentId)
    {
        return (await _dataAccessHelper.QueryData<DocumentModel, dynamic>("USP_Document_GetById", new { Id = documentId })).FirstOrDefault();
    }


    public async Task<List<DocumentModel>> GetDocumentsByUserId(string userId)
    {
        return (await _dataAccessHelper.QueryData<DocumentModel, dynamic>("USP_Document_GetByUserId", new { userId = userId}));
		
	}

    public async Task<string> InsertDocument(DocumentModel document, LogModel logModel)
    {
        ClearCache(DocumentCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", document.Id);
        p.Add("UserId", document.UserId);
        p.Add("DocumentType", document.DocumentType);
        p.Add("DocumentUrl", document.DocumentUrl);
        //p.Add("ReferenceId", document.ReferenceId);
        //p.Add("DocumentClassType", document.DocumentClassType);
        //p.Add("Name", document.Name);
        //p.Add("FileSize", document.FileSize);
        //p.Add("FileType", document.FileType);
        //p.Add("FileData", document.FileData);
        p.Add("CreatedBy", document.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);
      
        await _dataAccessHelper.ExecuteData("USP_Document_Insert", p);       

        return document.Id;
    }

    public async Task UpdateDocument(DocumentModel document, LogModel logModel)
    {
        ClearCache(DocumentCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", document.Id);
        p.Add("UserId", document.UserId);
        p.Add("DocumentType", document.DocumentType);
        p.Add("DocumentUrl", document.DocumentUrl);
        p.Add("ModifiedBy", document.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Document_Update", p);
    }

    public async Task DeleteDocument(string documentId, LogModel logModel)
    {
        ClearCache(DocumentCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", documentId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Document_Delete", p);
    }

    public async Task<List<DocumentModel>> Export()
    {
        return await _dataAccessHelper.QueryData<DocumentModel, dynamic>("USP_Document_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DocumentCache:
                var keys = _cache.Get<List<string>>(DocumentCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(DocumentCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}