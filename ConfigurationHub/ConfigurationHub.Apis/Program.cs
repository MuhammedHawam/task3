using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PartnersHub.ConfigurationHub.Apis.Middleware;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Repositories;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Middleware.Interfaces;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence.Services;
using PartnersHub.ConfigurationHub.Infrastructure.Presistence.Repositories;
using PartnersHub.ConfigurationHub.Infrastructure.Presistence.Services;
using PartnersHub.ConfigurationHub.Infrastructure.Services;
using System.Security.Cryptography;
using IModuleRepository = PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence.IModuleRepository;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ConfigurationHub API", Version = "v1" });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(PartnersHub.ConfigurationHub.Application.WhiteListIPs.Commands.CreateWhiteListIPCommand).Assembly);
});

builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<ConfigurationHubDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=(localdb)\\MSSQLLocalDB;Database=ConfigurationHubDB;Trusted_Connection=True;");
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// Repositories
builder.Services.AddScoped<IWhiteListIPRepository, WhiteListIPRepository>();
builder.Services.AddScoped<ITermsAndConditionRepository, TermsAndConditionRepository>();
builder.Services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
builder.Services.AddScoped<ISectorRepository, SectorRepository>();
builder.Services.AddScoped<ISubSectorRepository, SubSectorRepository>();
builder.Services.AddScoped<IAssetTypeRepository, AssetTypeRepository>();
builder.Services.AddScoped<IUnitOfMeasurementRepository, UnitOfMeasurementRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IRegisteredCompanyRepository, RegisteredCompanyRepository>();
// Services
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IScimUserService, ScimUserService>();
builder.Services.AddScoped<ILdapUserService, LdapUserService>();
builder.Services.AddScoped<ITokenSourceService, TokenSourceService>();
builder.Services.AddScoped<IMiddlewareCompanyService, MiddlewareCompanyService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HTTP Client
builder.Services.AddHttpClient(ScimApiConstants.ClientName, client =>
{
    var scimBaseUrl = builder.Configuration["Scim:BaseUrl"] ?? "https://ciam-uat.pif.gov.sa";
    client.BaseAddress = new Uri(scimBaseUrl);
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

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfigurationHub API V1");
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

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize Database
await InitializeDatabaseAsync(app);

app.Run();

// Authentication Configuration Methods
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
    var issuer = builder.Configuration["CiamSso:Issuer"] ;
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

async Task InitializeDatabaseAsync(WebApplication application)
{
    try
    {
        using var scope = application.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationHubDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied");
        }

        //await DbInitializer.InitializeDatabaseAsync(dbContext);
        await RulesEngineSeeder.SeedRbacDataAsync(dbContext);
        await DefaultSuperAdminSeeder.AssignDefaultSuperAdminAsync(dbContext, configuration, logger);
        await TestUsersSeeder.SeedTestUsersAsync(dbContext, configuration, logger);
        
        logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        application.Logger.LogError(ex, "Database initialization failed");
    }
}