using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Persistence;

public interface IEmailTemplateRepository
{
    Task<PaginatedListModel<EmailTemplateModel>> GetEmailTemplates(int pageNumber);
    Task<EmailTemplateModel> GetEmailTemplateById(string emailTemplateId);
    Task<EmailTemplateModel> GetEmailTemplateByName(string name);
    Task UpdateEmailTemplate(EmailTemplateModel emailTemplate, LogModel logModel);
}