using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Infrastructure;

public interface IEmailSender
{
    Task SendEmail(EmailModel email);
}