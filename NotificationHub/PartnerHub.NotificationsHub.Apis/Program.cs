using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PartnerHub.NotificationsHub.Apis.Handlers;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Options;
using PartnerHub.NotificationsHub.Application.Services;
using PartnerHub.NotificationsHub.Infrastructure.Persistence;
using PartnerHub.NotificationsHub.Infrastructure.Repositories;
using PartnerHub.NotificationsHub.Infrastructure.Workers;
using PartnerHub.NotificationsHub.Infrastructure.Services;
using System.Security.Cryptography;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NotificationsDb"));
});

// Configuration
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<NotificationRetryOptions>(
    builder.Configuration.GetSection("NotificationRetry"));

// Services
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<ISmsSender, ConsoleSmsSender>();
builder.Services.AddSingleton<IWebNotificationSender, SignalRWebNotificationSender>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();


#region Authentication 
const string ADFS_SCHEME = "ActiveDirectoryScheme";
const string CIAM_SCHEME = "CiamSsoScheme";
const string EXTERNAL_ALT_SCHEME = "ExternalPortalScheme";

var jwksUri = builder.Configuration["Authentication:ActiveDirectory:JwksUri"] ??
               "https://testadfs.pif.gov.sa/adfs/discovery/keys";

SecurityKey[] adfsSigningKeys = Array.Empty<SecurityKey>();

try
{
    using var httpClient = new HttpClient();
    var jwksJson = await httpClient.GetStringAsync(jwksUri);
    var jwks = new JsonWebKeySet(jwksJson);
    adfsSigningKeys = jwks.GetSigningKeys().ToArray();
}
catch (Exception ex)
{
    builder.Logging.AddConsole().AddDebug();
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Failed to retrieve ADFS signing keys: {Message}", ex.Message);
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiAuth";
})
.AddJwtBearer(ADFS_SCHEME, ConfigureAdfsAuthentication)
.AddJwtBearer(CIAM_SCHEME, ConfigureCiamAuthentication)
.AddJwtBearer(EXTERNAL_ALT_SCHEME, ConfigureExternalAltAuthentication)
.AddScheme<AuthenticationSchemeOptions, MultiAuthHandler>("MultiAuth", null);
#endregion

builder.Services.AddHostedService<NotificationRetryWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

// Remove sensitive headers
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        return Task.CompletedTask;
    });
    await next();
});

app.UseRouting();
app.UseCors("AllowedOrigins");
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();


void ConfigureAdfsAuthentication(JwtBearerOptions options)
{
    var validIssuers = builder.Configuration
        .GetSection("Authentication:ActiveDirectory:ValidIssuers")
        .Get<string[]>();

    var validAudiences = builder.Configuration
        .GetSection("Authentication:ActiveDirectory:ValidAudiences")
        .Get<string[]>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = validIssuers,
        ValidateAudience = true,
        ValidAudiences = validAudiences,
        ValidateLifetime = true,
        IssuerSigningKeys = adfsSigningKeys.Length > 0 ? adfsSigningKeys : null,
    };
}
void ConfigureCiamAuthentication(JwtBearerOptions options)
{
    var issuer = builder.Configuration["CiamSso:Issuer"];
    var audience = builder.Configuration["CiamSso:Audience"];

    options.Authority = builder.Configuration["CiamSso:Authority"] ?? issuer;
    options.Audience = audience;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = issuer,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        NameClaimType = "preferred_username"
    };
}

void ConfigureExternalAltAuthentication(JwtBearerOptions options)
{
    var issuer = builder.Configuration["Authentication:ExternalPortal:Issuer"];
    var audience = builder.Configuration["Authentication:ExternalPortal:Audience"];

    var publicKey = builder.Configuration["Authentication:ExternalPortal:PublicKey"];
    var rsa = RSA.Create();
    rsa.ImportRSAPrivateKey(Convert.FromBase64String(builder.Configuration["Authentication:ExternalPortal:PrivateKey"]), out _);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new RsaSecurityKey(rsa)
    };
}
