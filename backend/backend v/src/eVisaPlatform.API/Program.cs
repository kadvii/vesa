using eVisaPlatform.API.BackgroundServices;
using eVisaPlatform.API.Middleware;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Application.Mappings;
using eVisaPlatform.Application.Validators;
using eVisaPlatform.Infrastructure.Data;
using eVisaPlatform.Infrastructure.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/evisa-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ─── Infrastructure (DB, Auth, Repos, Services) ─────────────────────────────
builder.Services.AddInfrastructureServices(builder.Configuration);

// ─── AutoMapper ─────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// ─── FluentValidation ────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

// ─── Rate limiting (auth credential endpoints only — see AuthController) ────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("Auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddOutputCache();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

builder.Services.AddHostedService<RefreshTokenCleanupService>();

// ─── HttpContextAccessor ─────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ─── Swagger / OpenAPI ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "eVisa Platform API",
        Version = "v1",
        Description = "Production-ready Electronic Visa Management System API",
        Contact = new OpenApiContact { Name = "eVisa Team" }
    });

    // JWT Bearer auth in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ─── CORS: explicit origins (dashboard); localhost merged in Development ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins:Dashboard")
            .Get<string[]>()
            ?.Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        if (builder.Environment.IsDevelopment())
        {
            foreach (var dev in new[]
                     {
                         "http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:5173",
                         "https://localhost:5173", "http://localhost:4200"
                     })
                if (!origins.Contains(dev, StringComparer.OrdinalIgnoreCase))
                    origins.Add(dev);
        }

        if (origins.Count == 0)
            throw new InvalidOperationException(
                "Configure AllowedOrigins:Dashboard (array of origins). Required in Production.");

        policy.WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ─── Development database seed (empty DB only) ───────────────────────────────
if (app.Environment.IsDevelopment())
{
    await using var seedScope = app.Services.CreateAsyncScope();
    var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordService = seedScope.ServiceProvider.GetRequiredService<IPasswordService>();
    await DatabaseSeeder.SeedAsync(db, passwordService);
}

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eVisa Platform API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve wwwroot/uploads
app.UseCors("Dashboard");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
