using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence
{
    public interface IEmailSendingRepository
    {
        Task<PaginatedListModel<EmailSendingModel>> GetEmailSendings(int pageNumber);
        Task<EmailSendingModel> GetEmailSendingById(string emailSendingId);
        Task<string> InsertEmailSending(EmailSendingModel emailSending, LogModel logModel);
        Task UpdateEmailSending(EmailSendingModel emailSending, LogModel logModel);
        Task DeleteEmailSending(string emailSendingId, LogModel logModel);
        Task<List<EmailSendingModel>> Export();
    }
}
