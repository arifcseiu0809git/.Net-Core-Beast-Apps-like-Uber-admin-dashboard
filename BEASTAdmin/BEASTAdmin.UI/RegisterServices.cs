using AspNetCore.ReCaptcha;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BEASTAdmin.Service;
using BEASTAdmin.Service.Vehicle;
using BEASTAdmin.Service.Base;

using BEASTAdmin.UI.Areas.Identity.Data;
using BEASTAdmin.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

namespace BEASTAdmin.UI;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("SMSAPI", c => { c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("SMSSettings:SMSBaseAPIAddress")); });

        builder.Services.AddDbContext<MembershipDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MembershipDatabase")));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;

            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 1000;
            options.Lockout.AllowedForNewUsers = true;

            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#";
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<MembershipDbContext>()
        .AddDefaultUI()
        .AddDefaultTokenProviders();

        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews(); // TODO: May be for Identity, not sure
        builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));
        builder.Services.AddMemoryCache();
        builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                    .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
        builder.Services.AddAntiforgery(option => option.HeaderName = "XSRF-TOKEN");
        
        builder.Services.AddHttpClient("ServiceAPI", c => { c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseAPIAddress")); });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IContextAccessor, ContextAccessor>();
        builder.Services.AddSingleton<SecurityHelper>();
        builder.Services.AddScoped<EmailService>();
        builder.Services.AddScoped<SMSService>();
        builder.Services.AddScoped<AuditLogService>();
        builder.Services.AddScoped<ApplicationLogService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<SystemStatusService>();
        builder.Services.AddScoped<CategoryService>();
        builder.Services.AddScoped<PieService>();
        builder.Services.AddScoped<EmailTemplateService>();
        builder.Services.AddScoped<VehicleTypeService>();
        builder.Services.AddScoped<PassengerService>();
        builder.Services.AddScoped<TransactionService>();
        builder.Services.AddScoped<TransactionDetailService>();
        builder.Services.AddScoped<TransactionRequestService>();
        builder.Services.AddScoped<TransactionResponseService>();

        builder.Services.AddScoped<SavedAddressService>();
        builder.Services.AddScoped<DriverModelService>();
        builder.Services.AddScoped<VehicleFareService>();
        builder.Services.AddScoped<VehicleBrandService>();
        builder.Services.AddScoped<VehicleModelService>();

        builder.Services.AddScoped<PaymentMethodService>();
        builder.Services.AddScoped<CouponService>();
        builder.Services.AddScoped<CityService>();
        builder.Services.AddScoped<CountryService>();
        builder.Services.AddScoped<PaymentOptionService>();
        builder.Services.AddScoped<PaymentTypeService>();

        builder.Services.AddScoped<VehicleBrandService>();
        builder.Services.AddScoped<VehicleCurrentLocationService>();
        builder.Services.AddScoped<VehiclesService>();
        builder.Services.AddScoped<SavedAddressService>();
        builder.Services.AddScoped<DriverModelService>();
        builder.Services.AddScoped<VehicleFareService>();
        builder.Services.AddScoped<VehicleModelService>(); 
        builder.Services.AddScoped<CouponService>();
        builder.Services.AddScoped<CityService>();
        builder.Services.AddScoped<CountryService>();
        builder.Services.AddScoped<TripService>();
        builder.Services.AddScoped<TripInitialService>();
        builder.Services.AddScoped<DocumentService>();
        builder.Services.AddScoped<DocumentTypeService>();
        builder.Services.AddScoped<AdminEarningService>();
        builder.Services.AddScoped<XDriverVehicleService>();
    }
}