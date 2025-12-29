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
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Resources;
using PartnersHub.Synergy.Infrastructure.Persistence;
using PartnersHub.Synergy.Infrastructure.Persistence.Interfaces;
using PartnersHub.Synergy.Infrastructure.Persistence.Repositories;
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
// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.Synergy.Application.SynergyCompany.Queries.GetRegisteredCompaniesQuery).Assembly);
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
    var notificationBaseUrl = builder.Configuration["Norification:BaseUrl"];
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

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.Synergy.Application.AssemblyReference).Assembly);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
using var scope = app.Services.CreateScope();




// Configure the HTTP request pipeline.
//TODO: uncomment the below on UAT
//if (app.Environment.IsDevelopment())
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
            Guid.NewGuid(),
            Convert.FromBase64String("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")
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
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");
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

