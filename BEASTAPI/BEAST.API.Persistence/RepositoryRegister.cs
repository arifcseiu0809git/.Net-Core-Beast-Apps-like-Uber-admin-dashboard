using BEASTAPI.Persistence.Common;
using BEASTAPI.Persistence.Map;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence.Common;
using BEASTAPI.Core.Contract.Persistence.Map;
using BEASTAPI.Core.Contract.Persistence.Passenger;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BEASTAPI.Persistence.Passenger;
using BEASTAPI.Persistence.Transaction;
using BEASTAPI.Core.Contract.Persistence.Transaction;
using BEASTAPI.Core.Contract.Vehicle;
using BEAST.API.Persistence;
using BEASTAPI.Core.Contract.Driver;

namespace BEASTAPI.Persistence
{
    public static class RepositoryRegister
    {
        public static IServiceCollection AddRepositoriesDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDataAccessHelper, DataAccessHelper>();
            services.AddSingleton<ISecurityHelper, SecurityHelper>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
            services.AddScoped<ICsvExporter, CsvExporter>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IPieRepository, PieRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IPricingRepository, PricingRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<ISystemStatusRepository, SystemStatusRepository>();
            services.AddScoped<ITripInitialRepository, TripInitialRepository>();
            services.AddScoped<IMobileAuthRepository, MobileAuthRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IPassengerAuthRepository, PassengerAuthRepository>();
            services.AddScoped<IPassengersLocationRepository, PassengersLocationRepository>();
            services.AddScoped<IPassengersPaymentMethodRepository, PassengersPaymentMethodRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IVehicleBrandRepository, VehicleBrandRepository>();
            services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
            services.AddScoped<IVehicleCurrentLocationRepository, VehicleCurrentLocationRepository>();
            services.AddScoped<IVehicleFareRepository, VehicleFareRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<ISMSRepository, SMSRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionDetailRepository, TransactionDetailRepository>();
            services.AddScoped<ITransactionRequestRepository, TransactionRequestRepository>();
            services.AddScoped<ITransactionResponseRepository, TransactionResponseRepository>();
            services.AddScoped<ISavedAddressRepository, SavedAddressRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IEmailSendingRepository, EmailSendingRepository>();
            services.AddScoped<IPaymentOptionRepository, PaymentOptionRepository>();
            services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IAdminEarningRepository, AdminEarningRepository>();
            services.AddScoped<IXDriverVehicleRepository, XDriverVehicleRepository>();
            return services;
        }
    }
}
