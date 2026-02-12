using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PartnersHub.InnovationHub.Apis.Common;
using PartnersHub.InnovationHub.Apis.Middleware;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Services;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Infrastructure.Persistence;
using PartnersHub.InnovationHub.Infrastructure.Presistence;
using PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;
using PartnersHub.InnovationHub.Infrastructure.Presistence.Services;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Serilog;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Integration;
using PartnersHub.InnovationHub.Infrastructure.Presistence.Services.Integration;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// FIX 1: Add HttpContextAccessor to resolve dependency error in AdminCommunicationService
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.IncludeFields = true;
        //options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.InnovationHub.Application.AssemblyReference).Assembly);
});

builder.Services.AddDbContext<InnovationHubDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=(localdb)\\MSSQLLOCALDB;Database=InnovationHubDB;Trusted_Connection=True;",
        b => b.MigrationsAssembly(typeof(InnovationHubDbContext).Assembly.FullName));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

builder.Services.AddScoped<IChallengeRequestRepository, ChallengeRequestRepository>();
builder.Services.AddScoped<IChallengeTechnologiesRequestRepository, ChallengeTechnologiesRequestRepository>();
builder.Services.AddScoped<ITechnologyRepository, TechnologyRepository>();
builder.Services.AddScoped<IAssociatedProviderRepository, AssociatedProviderRepository>();
builder.Services.AddScoped<IAssociatedSectorRepository, AssociatedSectorRepository>();
builder.Services.AddScoped<IReviewChallengeRepository, ReviewChallengeRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<ICampaignRequestRepository, CampaignRequestRepository>();
builder.Services.AddScoped<ICampaignRequestLinkedChallengeRepository, CampaignRequestLinkedChallengeRepository>();
builder.Services.AddScoped<ICampaignRequestSponsorRepository, CampaignRequestSponsorRepository>();
builder.Services.AddScoped<ICampaignRequestTermsAndConditionRepository, CampaignRequestTermsAndConditionRepository>();



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


builder.Services.AddScoped<ICampaignRequestEvaluationCriteriaRepository, CampaignRequestEvaluationCriteriaRepository>();
builder.Services.AddScoped<ICampaignRequestEvaluatorRepository, CampaignRequestEvaluatorRepository>();
builder.Services.AddScoped<ISponsorRepository, SponsorRepository>();
builder.Services.AddScoped<IEvaluatorRepository, EvaluatorRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMiddlewareIntegrationService, MiddlewareIntegrationService>();


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

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient(Constants.NotificationClient, client =>
{
    var notificationBaseUrl = builder.Configuration["NotificationSettings:BaseUrl"];
    client.BaseAddress = new Uri(notificationBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.Configure<EmailParameters>(builder.Configuration.GetSection("EmailParameters"));
builder.Services.AddSwaggerGen(c =>
{
   // c.SwaggerDoc("v1", new() { Title = "Partners Hub Synergy API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

//TODO: uncomment the below on UAT
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

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

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();


try
{
    // Use 'using var' and CreateAsyncScope for safe, automatic, asynchronous disposal 
    // of the service scope and its dependencies (like the DbContext).
    using var scope = app.Services.CreateAsyncScope();

    // Get the database context from the service provider
    var context = scope.ServiceProvider.GetRequiredService<InnovationHubDbContext>();
    var database = context.Database;

    app.Logger.LogInformation("Checking for pending database migrations...");

    // Asynchronously check if there are any migrations that haven't been applied yet
    if ((await database.GetPendingMigrationsAsync()).Any())
    {
        app.Logger.LogInformation("Applying pending migrations...");

        // Asynchronously apply all pending migrations to the database
        await database.MigrateAsync();

        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    else
    {
        app.Logger.LogInformation("Database is already up to date.");
    }
}
catch (Exception ex)
{
    // Log the error as Critical, since a failed migration on startup is application-stopping
    app.Logger.LogCritical(ex,
        "FATAL ERROR: An unrecoverable error occurred while migrating the database. " +
        "This usually means a migration is trying to CREATE an object that already exists. Details: {Message}",
        ex.Message);

    // Re-throw the exception to halt the application startup, as the database is in an inconsistent state.
    throw;
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
