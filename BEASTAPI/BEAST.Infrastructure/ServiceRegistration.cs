using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Model;

namespace BEASTAPI.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //services.Configure<EmailSettingsRegularModel>(configuration.GetSection("EmailSettings_Regular"));
        //services.AddScoped<IEmailSender, EmailSenderRegular>();
        
        services.Configure<EmailSettingsSendGridModel>(configuration.GetSection("EmailSettings_SendGrid"));
        services.AddScoped<IEmailSender, EmailSenderSendGrid>();

        services.Configure<SMSSettingsModel>(configuration.GetSection("SMSSettings"));
        services.AddScoped<ISMSSender, SMSSenderAlpha>();

        return services;
    }
}