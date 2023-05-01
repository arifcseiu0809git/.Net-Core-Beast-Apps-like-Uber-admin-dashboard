using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;
using System.Data;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence.Common;

public class MessageRepository : IMessageRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string MessageCache = "MessageData";

    public MessageRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    #region "DataAccessHelper Methods"
    public async Task<PaginatedListModel<MessageModel>> GetMessages(int pageNumber)
    {
        PaginatedListModel<MessageModel> output = _cache.Get<PaginatedListModel<MessageModel>>(MessageCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<MessageModel, dynamic>("USP_Message_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<MessageModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(MessageCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(MessageCache);
            if (keys is null)
                keys = new List<string> { MessageCache + pageNumber };
            else
                keys.Add(MessageCache + pageNumber);
            _cache.Set(MessageCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<MessageModel> GetMessageById(string messageId)
    {
        return (await _dataAccessHelper.QueryData<MessageModel, dynamic>("USP_Message_GetById", new { Id = messageId })).FirstOrDefault();
    }


    public async Task<MessageModel> GetMessageByName(string messageName)
    {
        return (await _dataAccessHelper.QueryData<MessageModel, dynamic>("USP_Message_GetByName", new { Name = messageName })).FirstOrDefault();
    }

    public async Task<string> InsertMessage(MessageModel message, LogModel logModel)
    {
        ClearCache(MessageCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", message.Id);
        p.Add("TitleText", message.TitleText);
        p.Add("DescriptionText", message.DescriptionText);
        p.Add("IconName", message.IconName);
        p.Add("Param", message.Param);
        p.Add("FromUserId", message.FromUserId);
        p.Add("ToUserId", message.ToUserId);
        p.Add("ExternalLink", message.ExternalLink);
        p.Add("IsSeen", message.IsSeen);
        p.Add("SeenTime", message.SeenTime);
        p.Add("CreatedBy", message.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Message_Insert", p);
        return message.Id;
    }

    public async Task UpdateMessage(MessageModel message, LogModel logModel)
    {
        ClearCache(MessageCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", message.Id);
        p.Add("TitleText", message.TitleText);
        p.Add("DescriptionText", message.DescriptionText);
        p.Add("IconName", message.IconName);
        p.Add("Param", message.Param);
        p.Add("FromUserId", message.FromUserId);
        p.Add("ToUserId", message.ToUserId);
        p.Add("ExternalLink", message.ExternalLink);
        p.Add("IsSeen", message.IsSeen);
        p.Add("SeenTime", message.SeenTime);
        p.Add("LastModifiedBy", message.ModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Message_Update", p);
    }

    public async Task DeleteMessage(string messageId, LogModel logModel)
    {
        ClearCache(MessageCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", messageId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Message_Delete", p);
    }

    public async Task<List<MessageModel>> Export()
    {
        return await _dataAccessHelper.QueryData<MessageModel, dynamic>("USP_Message_Export", new { });
    }
    #endregion

    #region "Helper Methods"
    private void ClearCache(string key)
    {
        switch (key)
        {
            case MessageCache:
                var keys = _cache.Get<List<string>>(MessageCache);
                if (keys is not null)
                {
                    foreach (var item in keys)
                        _cache.Remove(item);
                    _cache.Remove(MessageCache);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}