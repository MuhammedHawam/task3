using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Infrastructure.Presistence;
using PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;


namespace PartnersHub.Services.InnovationHub.IntegrationTests;

/// <summary>
/// Integration tests for InnovationHub - tests the full stack with real database operations
/// Uses in-memory database for fast, isolated testing
/// </summary>
public class InnovationHubIntegrationTests {
    private InnovationHubDbContext _context = null!;
    private IChallengeRequestRepository _repository = null!;
    private IUnitOfWork _unitOfWork = null!;

    [SetUp]
    public void Setup() {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<InnovationHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"InnovationHubTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new InnovationHubDbContext(options);
        _repository = new ChallengeRequestRepository(_context);
      //  _unitOfWork = new UnitOfWork(_context);
    }

    [TearDown]
    public void TearDown() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

  
}
