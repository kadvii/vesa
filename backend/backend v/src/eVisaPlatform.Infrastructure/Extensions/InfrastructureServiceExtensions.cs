using eVisaPlatform.Application.Configuration;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Application.Services;
using eVisaPlatform.Infrastructure.Data;
using eVisaPlatform.Infrastructure.Repositories;
using eVisaPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace eVisaPlatform.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("eVisaPlatform.Infrastructure")));

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // ── Repositories & Unit of Work ───────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── SMTP / Email ──────────────────────────────────────────────────────
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddSingleton<IEmailService, SmtpEmailService>();

        // ── DateTime Abstraction ──────────────────────────────────────────────
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // ── Infrastructure Services ───────────────────────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, BcryptPasswordService>();   // ← BCrypt (no Identity)

        // ── Application Services ──────────────────────────────────────────────
        services.AddScoped<IAuthService,               AuthService>();
        services.AddScoped<IUserService,               UserService>();
        services.AddScoped<IVisaApplicationService,    VisaApplicationService>();
        services.AddScoped<IDocumentService,           DocumentService>();
        services.AddScoped<INotificationService,       NotificationService>();
        services.AddScoped<IFamilyVisaService,         FamilyVisaService>();
        services.AddScoped<ISupportService,            SupportService>();
        services.AddScoped<ITravelConsultantService,   TravelConsultantService>();
        services.AddScoped<IVisaAgentService,          VisaAgentService>();
        services.AddScoped<IVisaGuaranteeService,      VisaGuaranteeService>();
        services.AddScoped<IPaymentService,            PaymentService>();
        services.AddScoped<IAppointmentService,        AppointmentService>();
        services.AddScoped<IAuditLogService,           AuditLogService>();
        services.AddScoped<IReportRepository,          ReportRepository>();
        services.AddScoped<IReportService,             ReportService>();
        services.AddScoped<ISettingService,            SettingService>();

        // ── JWT Authentication ────────────────────────────────────────────────
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey   = jwtSettings["SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException(
                "JWT SecretKey is not configured. " +
                "Set it via User Secrets (development) or the " +
                "'JwtSettings__SecretKey' environment variable (production).");

        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                ValidateIssuer           = true,
                ValidIssuer              = jwtSettings["Issuer"],
                ValidateAudience         = true,
                ValidAudience            = jwtSettings["Audience"],
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };
        });

        // ── Authorization Policies ────────────────────────────────────────────
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",        p => p.RequireRole("Admin"));
            options.AddPolicy("AdminOrEmployee",  p => p.RequireRole("Admin", "Employee"));
            options.AddPolicy("AgentAccess",      p => p.RequireRole("Admin", "Agent"));
            options.AddPolicy("ConsultantAccess", p => p.RequireRole("Admin", "Consultant"));
        });

        return services;
    }
}
