using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PartnersHub.infraBase.Apis.Middleware;
using PartnersHub.InfraBase.Apis.Common;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Interfaces.Services;
using PartnersHub.InfraBase.Application.Common.Options;
using PartnersHub.InfraBase.Application.Common.Services;
using PartnersHub.InfraBase.Domain.Common;
using PartnersHub.InfraBase.Infrastructure.Persistence;
using PartnersHub.InfraBase.Infrastructure.Persistence.Repositories;
using PartnersHub.InfraBase.Infrastructure.Services;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableGuidConverter());
        options.JsonSerializerOptions.Converters.Add(new GuidConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add Configuration Options
builder.Services.Configure<AssetCodeSettings>(
    builder.Configuration.GetSection("AssetCodeSettings"));

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.InfraBase.Application.AssemblyReference).Assembly);
});

// Add Database Context
//builder.Services.AddDbContext<InfrabaseDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        b => b.MigrationsAssembly("PartnersHub.InfraBase.Infrastructure")));

builder.Services.AddDbContext<InfrabaseDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=(localdb)\\MSSQLLocalDB;Database=InfraBaseDb-Dev;Trusted_Connection=True;",
        b => b.MigrationsAssembly(typeof(InfrabaseDbContext).Assembly.FullName));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// Add Repositories
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


builder.Services.AddHttpClient(Constants.NotificationClient, client =>
{
    var notificationBaseUrl = builder.Configuration["NotificationSettings:BaseUrl"];
    client.BaseAddress = new Uri(notificationBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
//builder.Services.AddScoped<IAdminCommunicationService, AdminCommunicationService>();

//builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
//builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
//builder.Services.AddScoped<PermissionCacheService>();

//builder.Services.AddHttpClient(AdminpiConstants.ClientName, client =>
//{
//    var baseUrl = builder.Configuration["ConfigurationHub:BaseUrl"];
//    client.BaseAddress = new Uri(baseUrl!);
//});

// Add Notification Service (placeholder implementation)
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add Email Template Service
builder.Services.AddScoped<EmailTemplateService>();

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options => {
    options.AddPolicy("AllowedOrigins", policy => {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<EmailParameters>(builder.Configuration.GetSection("EmailParameters"));
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
        Title = "InfraBase API",
        Version = "v1",
        Description = "API for managing infrastructure assets and approvals"
    });

    // Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddHttpClient<IConfigurationLookupService, ConfigurationLookupService>(client =>
{
    var baseUrl = builder.Configuration["ConfigurationHub:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

builder.Services.AddHttpClient("MiddlewareApi", client =>
{
    var middlewareBaseUrl = builder.Configuration["MiddlewareApi:BaseUrl"];
    if (!string.IsNullOrEmpty(middlewareBaseUrl))
    {
        client.BaseAddress = new Uri(middlewareBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    }
});
builder.Services.AddScoped<IMiddlewareIntegrationService, MiddlewareIntegrationService>();


builder.Services.AddHttpClient<IAdminCommunicationService, AdminCommunicationService>(client =>
{
    var baseUrl = builder.Configuration["ConfigurationHub:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

// Authentication Schemes
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();
var culture = new CultureInfo("en-SA");
var supportedCultures = new[] { culture };

var options = new RequestLocalizationOptions
{
DefaultRequestCulture = new RequestCulture(culture),
SupportedCultures = supportedCultures,
SupportedUICultures = supportedCultures,

// Optional: adds Content-Language: en-SA to the response headers
ApplyCurrentCultureToResponseHeaders = true
};

// Force en-SA and ignore ALL client-side culture sources (Accept-Language, cookies, etc.)
options.RequestCultureProviders = new IRequestCultureProvider[]
{
    new CustomRequestCultureProvider(_ =>
        Task.FromResult(new ProviderCultureResult("en-SA", "en-SA")))
};

app.UseRequestLocalization(options);
// Configure the HTTP request pipeline
//TODO: uncomment the below on UAT
//if (app.Environment.IsDevelopment())
{
app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InfraBase API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}

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

// Add global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowedOrigins");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    try {
        var context = services.GetRequiredService<InfrabaseDbContext>();
        context.Database.Migrate(); // Uncomment to auto-migrate
        app.Logger.LogInformation("Database connection verified");
    } catch (Exception ex) {
        app.Logger.LogError(ex, "An error occurred while connecting to the database");
    }
}

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
