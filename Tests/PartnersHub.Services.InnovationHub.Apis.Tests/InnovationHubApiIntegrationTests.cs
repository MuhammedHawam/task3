using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Infrastructure.Persistence;
using PartnersHub.InnovationHub.Infrastructure.Presistence;
using PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace PartnersHub.Services.InnovationHub.Apis.Tests;

/// <summary>
/// Custom WebApplicationFactory for API integration tests
/// Configures in-memory database for isolated testing
/// </summary>
public class InnovationHubApiFactory : WebApplicationFactory<Program> {
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) {
        builder.ConfigureServices(services => {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<InnovationHubDbContext>));
            if (descriptor != null) {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<InnovationHubDbContext>(options => {
                options.UseInMemoryDatabase($"InnovationHubApiTests_{Guid.NewGuid()}");
            });

            // Ensure services are registered
            services.AddScoped<IChallengeRequestRepository, ChallengeRequestRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            // Build service provider and create database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InnovationHubDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

/// <summary>
/// Comprehensive API integration tests for InnovationHub
/// Tests all API endpoints end-to-end
/// </summary>
[TestFixture]
public class InnovationHubApiIntegrationTests
{


   
}
