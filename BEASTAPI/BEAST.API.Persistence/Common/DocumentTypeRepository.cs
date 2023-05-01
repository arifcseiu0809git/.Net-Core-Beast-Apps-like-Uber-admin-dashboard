using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Persistence.Common;

public class DocumentTypeRepository : IDocumentTypeRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string DocumentTypeCache = "DocumentTypeData";

    public DocumentTypeRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<DocumentTypeModel>> GetDocumentTypes(int pageNumber)
    {
        PaginatedListModel<DocumentTypeModel> output = _cache.Get<PaginatedListModel<DocumentTypeModel>>(DocumentTypeCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<DocumentTypeModel, dynamic>("USP_DocumentType_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<DocumentTypeModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(DocumentTypeCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(DocumentTypeCache);
            if (keys is null)
                keys = new List<string> { DocumentTypeCache + pageNumber };
            else
                keys.Add(DocumentTypeCache + pageNumber);
            _cache.Set(DocumentTypeCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<DocumentTypeModel> GetDocumentTypeById(string DocumentTypeId)
    {
        return (await _dataAccessHelper.QueryData<DocumentTypeModel, dynamic>("USP_DocumentType_GetById", new { Id = DocumentTypeId })).FirstOrDefault();
    }

    //public async Task<List<DocumentTypeModel>> GetDistinctDocumentTypes()
    //{
    //    var output = _cache.Get<List<DocumentTypeModel>>(DocumentTypeCache);

    //    if (output is null)
    //    {
    //        output = await _dataAccessHelper.QueryData<DocumentTypeModel, dynamic>("USP_DocumentType_GetDistinct", new { });
    //        _cache.Set(DocumentTypeCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
    //    }

    //    return output;
    //}

	public async Task<string> InsertDocumentType(DocumentTypeModel DocumentType, LogModel logModel)
    {
        ClearCache(DocumentTypeCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", DocumentType.Id);
        p.Add("Name", DocumentType.Name);
        p.Add("DocumentFor", DocumentType.DocumentFor);
        p.Add("CreatedBy", DocumentType.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);
      
        await _dataAccessHelper.ExecuteData("USP_DocumentType_Insert", p);       

        return DocumentType.Id;
    }

    public async Task UpdateDocumentType(DocumentTypeModel DocumentType, LogModel logModel)
    {
        ClearCache(DocumentTypeCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", DocumentType.Id);
        p.Add("Name", DocumentType.Name);
        p.Add("DocumentFor", DocumentType.DocumentFor);
        p.Add("ModifiedBy", DocumentType.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_DocumentType_Update", p);
    }

    public async Task DeleteDocumentType(string DocumentTypeId, LogModel logModel)
    {
        ClearCache(DocumentTypeCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", DocumentTypeId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_DocumentType_Delete", p);
    }

    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case DocumentTypeCache:
                var keys = _cache.Get<List<string>>(DocumentTypeCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(DocumentTypeCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}