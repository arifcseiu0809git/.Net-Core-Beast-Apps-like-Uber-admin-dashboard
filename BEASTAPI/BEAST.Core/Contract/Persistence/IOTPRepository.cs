using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface IOTPRepository
{
    Task<string> InsertOTP(SMSModels sms, LogModel logModel);
    Task UpdateOTP(SMSModels sms, LogModel logModel);
    Task DeleteOTP(string smsId, LogModel logModel);
    Task<SMSModels> GetOTPById(string smsId);
    Task<PaginatedListModel<SMSModels>> GetOTPs(int pageNumber);
    Task<List<SMSModels>> Export();
}