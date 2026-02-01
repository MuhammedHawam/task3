using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PartnersHub.Synergy.Apis.Common;
using PartnersHub.Synergy.Apis.Middleware;
using PartnersHub.Synergy.Application.Common.Interfaces;
using PartnersHub.Synergy.Application.Common.Interfaces.Services;
using PartnersHub.Synergy.Application.Common.Options;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Interfaces.Repository.Dapper;
using PartnersHub.Synergy.Application.Interfaces.Services;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Resources;
using PartnersHub.Synergy.Infrastructure.Persistence;
using PartnersHub.Synergy.Infrastructure.Persistence.Interfaces;
using PartnersHub.Synergy.Infrastructure.Persistence.Repositories;
using PartnersHub.Synergy.Infrastructure.Persistence.StaticData;
using PartnersHub.Synergy.Infrastructure.Repositories;
using PartnersHub.Synergy.Infrastructure.Repositories.Dapper;
using PartnersHub.Synergy.Infrastructure.Services;
using PartnersHub.Synergy.Infrastructure.Services.Common;
using PartnersHub.Synergy.Infrastructure.Services.Integration;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add HttpContextAccessor for service access
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<EmailParameters>(builder.Configuration.GetSection("EmailParameters"));



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Partners Hub Synergy API", Version = "v1" });
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
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiAuth";
})
.AddJwtBearer(ADFS_SCHEME, ConfigureAdfsAuthentication)
.AddJwtBearer(CIAM_SCHEME, ConfigureCiamAuthentication)
.AddJwtBearer(EXTERNAL_ALT_SCHEME, ConfigureExternalAltAuthentication)
.AddScheme<AuthenticationSchemeOptions, MultiAuthHandler>("MultiAuth", null);

builder.Services.Configure<RequestCodeSettings>(
    builder.Configuration.GetSection("RequestCodeSettings"));
builder.Services.AddDbContext<SynergyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
builder.Services.AddSingleton<IDapperConnectionFactory, DapperConnectionFactory>();
builder.Services.AddHttpClient("MiddlewareApi", client =>
{
    // Configure default settings for this named client
    client.BaseAddress = new Uri(builder.Configuration["MiddlewareApi:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient(Constants.NotificationClient, client =>
{
    var notificationBaseUrl = builder.Configuration["NotificationSettings:BaseUrl"];
    client.BaseAddress = new Uri(notificationBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddMemoryCache();
builder.Services.AddTransient<ICacheWrapper, CacheWrapper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpClient<ICompanyIntegrationService, CompanyIntegrationService>();
builder.Services.AddScoped<IUserProfileDataIntegrationService,  UserProfileDataIntegrationService>();
builder.Services.AddScoped<HandlerMessages>();
builder.Services.AddLocalization();
builder.Services.AddScoped<ISuccessStoryRepository, SuccessStoryRepository>();  
builder.Services.AddScoped<ICollaborationRequirementRepository, CollaborationRequirementRepository>();
builder.Services.AddScoped<IExpectedOutcomesRepository, ExpectedOutcomesRepository>();
builder.Services.AddScoped<IOpportunityTypeRepository, OpportunityTypeRepository>();
builder.Services.AddScoped<ISynergyCompanyRepository, SynergyCompanyRepository>();
builder.Services.AddScoped<IThematicAreaRepository, ThematicAreaRepository>();
builder.Services.AddScoped<IOpportunityRepository, OpportunityRepository>();
builder.Services.AddScoped<ISuccessStoryRepository, SuccessStoryRepository>();
builder.Services.AddScoped<ISuccessStroyTypeRepository,  SuccessStroyTypeRepository>();
builder.Services.AddScoped<IDapperRepository, DapperRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IStaticCompanyPifOwnerInfoProvider, StaticCompanyPifOwnerInfoProvider>();


// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.Synergy.Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.Synergy.Application.SynergyCompany.Queries.GetRegisteredCompaniesQuery).Assembly);
});


// Add CORS
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

var app = builder.Build();
using var scope = app.Services.CreateScope();



// Configure the HTTP request pipeline.
//TODO: uncomment the below on UAT
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Partners Hub Synergy API V1");
        c.RoutePrefix = "swagger";
    });

    app.UseDeveloperExceptionPage();
}

var dbContext = scope.ServiceProvider.GetRequiredService<SynergyDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Checking for pending migrations...");
    var database = dbContext.Database;
    
    if (database.GetPendingMigrations().Any())
    {
        logger.LogInformation("Applying pending migrations...");
        await database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("Database is up to date.");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while migrating the database: {Message}", ex.Message);
    throw;
}
//foreach(var company in dbContext.SynergyCompanies)
//{
//    company.Activate(Guid.NewGuid());
//    dbContext.Update(company);
   
//}
await dbContext.SaveChangesAsync();
if (!dbContext.SynergyCompanies.Any())
{
    var technologySectorId = Guid.Parse("ab1c686e-1790-ed11-b4ed-0022480d5665");
    var materialsSectorId = Guid.Parse("d65bb688-1528-eb11-bae6-00155d0f021d");
    var realEstateId = Guid.Parse("af1c686e-1790-ed11-b4ed-0022480d5665");
    var financeId = Guid.Parse("e40d8fac-1528-eb11-bae6-00155d0f021d");
    var companies = new[]
    {
        SynergyCompany.Create(
            new Guid("39cf6b3a-e94f-ef11-a4c6-005056992b12"),
            "Saudi Company for Artificial Intelligence",
            "KSA",
            "Riyadh",
            "Leading in AI",
            "Jassir Ahmed",
            "CEO",
            "jassir.doe@masdar.ae",
            "+971 2 565 5656",
            Guid.NewGuid()
        ).Value,
        SynergyCompany.Create(
            new Guid("4c81fe62-e29e-ef11-a4c9-005056992b12"),
            "NEOM",
            "KSA",
            "Riyadh",
            "Real Estate leading company",
            "Jane Doe",
            "CTO",
            "jane.doe@tasaru.com",
            "+971 4 123 4567",
            Guid.NewGuid()
        ).Value,

        SynergyCompany.Create(
            new Guid("395f4b96-e64f-ef11-a4c6-005056992b12"),
            "SURJ",
            "Egypt",
            "Cairo",
            "Sports institution",
            "Ronit Sela",
            "Director",
            "ronit.sela@surj.org.eg",
            "+202 2 123 4567",
            Guid.NewGuid()
        ).Value,
        SynergyCompany.Create(
            new Guid("3375f62d-f3c2-f011-a4de-005056992b12"),
            "PIF Community",
            "KSA",
            "Riyadh",
            "Finance",
            "Ahmed Jassir",
            "Director",
            "feq.sela@surj.org.eg",
            "+202 2 123 4567",
            Guid.NewGuid()
        ).Value
    };

    await dbContext.SynergyCompanies.AddRangeAsync(companies);
    await dbContext.SaveChangesAsync();

    companies[0].AddSectors(new Dictionary<Guid, string> 
    { 
        { technologySectorId, "Information Technology" } 
    });
    companies[1].AddSectors(new Dictionary<Guid, string> 
    { 
        { realEstateId, "Real Estate" } 
    });
    companies[2].AddSectors(new Dictionary<Guid, string> 
    { 
        { materialsSectorId, "Materials" } 
    });
    companies[3].AddSectors(new Dictionary<Guid, string>
    {
        { financeId, "Finance" }
    });

    await dbContext.SaveChangesAsync();
}
var cacheService = scope.ServiceProvider.GetRequiredService<ICacheWrapper>();
await cacheService.LoadLookupsIntoCacheAsync();

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

