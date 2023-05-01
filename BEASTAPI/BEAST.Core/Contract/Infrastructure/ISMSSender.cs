using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Contract.Infrastructure;

public interface ISMSSender
{
    Task SendSMS(SMSModel sms);
}