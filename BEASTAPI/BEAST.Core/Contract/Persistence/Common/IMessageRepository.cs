using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Contract.Persistence.Common;

public interface IMessageRepository
{
    Task<PaginatedListModel<MessageModel>> GetMessages(int pageNumber);
    Task<MessageModel> GetMessageById(string messageId);
    Task<MessageModel> GetMessageByName(string messageName);
    Task<string> InsertMessage(MessageModel message, LogModel logModel);
    Task UpdateMessage(MessageModel message, LogModel logModel);
    Task DeleteMessage(string messageId, LogModel logModel);
    Task<List<MessageModel>> Export();
}