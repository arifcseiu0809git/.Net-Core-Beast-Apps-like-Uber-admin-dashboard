using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface ISMSRepository
{
    Task<string> InsertSMS(SMSModels sms, LogModel logModel);
    Task UpdateSMS(SMSModels sms, LogModel logModel);
    Task DeleteSMS(string smsId, LogModel logModel);
    Task<SMSModels> GetSMSById(string smsId);
    Task<PaginatedListModel<SMSModels>> GetSMSes(int pageNumber);
    Task<List<SMSModels>> Export();
}