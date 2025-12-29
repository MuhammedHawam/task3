using MediatR;
using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Domain.Aggregates.CampaignRequest;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Aggregates;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using PartnersHub.InnovationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence;

public class InnovationHubDbContext : DbContext
{
    private readonly IMediator? _mediator;
    public InnovationHubDbContext(DbContextOptions<InnovationHubDbContext> options)
        : base(options)
    {
    }

    public InnovationHubDbContext(
           DbContextOptions<InnovationHubDbContext> options,
           IMediator mediator)
           : base(options)
    {
        _mediator = mediator;
    }
    public DbSet<ChallengeRequest> challengeRequests => Set<ChallengeRequest>();
    public DbSet<ChallengeRequestRevisionComment> challengeRequestRevisionComments => Set<ChallengeRequestRevisionComment>();
    public DbSet<ChallengeTrackingHistory> challengeTrackingHistories => Set<ChallengeTrackingHistory>();
    public DbSet<ChallengeTechnologiesRequest> challengeTechnologiesRequests => Set<ChallengeTechnologiesRequest>();
    public DbSet<Technology> technologies => Set<Technology>();
    public DbSet<ChallengeRequestAssociatedProvider> associatedProviders => Set<ChallengeRequestAssociatedProvider>();
    public DbSet<ChallengeRequestAssociatedSector> associatedSectors => Set<ChallengeRequestAssociatedSector>();
    public DbSet<ChallengeRequestAttachment> attachments => Set<ChallengeRequestAttachment>();
    public DbSet<CampaignRequest> campaignRequests => Set<CampaignRequest>();
    public DbSet<CampaignRequestLinkedChallenge> campaignRequestLinkedChallenges => Set<CampaignRequestLinkedChallenge>();
    public DbSet<CampaignRequestSponsor> campaignRequestSponsors => Set<CampaignRequestSponsor>();
    public DbSet<CampaignRequestTermsAndCondition> campaignRequestTermsAndConditions => Set<CampaignRequestTermsAndCondition>();
    public DbSet<CampaignRequestEvaluator> CampaignRequestEvaluator => Set<CampaignRequestEvaluator>();
    public DbSet<CampaignRequestEvaluationCriteria> CampaignRequestEvaluationCriteria => Set<CampaignRequestEvaluationCriteria>();
    public DbSet<CampaignTrackingHistory> campaignTrackingHistories => Set<CampaignTrackingHistory>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Evaluator> Evaluators => Set<Evaluator>(); 
    public DbSet<CompanySector> CompanySectors => Set<CompanySector>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ignore domain events - they should not be persisted
        modelBuilder.Ignore<DomainEvent>();

        // Apply all entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entitiesWithEvents = ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (_mediator != null)
            {
                foreach (var entity in entitiesWithEvents)
                {
                    var events = entity.DomainEvents.ToArray();

                    foreach (var domainEvent in events)
                    {
                        await _mediator.Publish(domainEvent, cancellationToken);
                    }

                    entity.ClearDomainEvents();
                }
            }
            else
            {
                foreach (var entity in entitiesWithEvents)
                {
                    entity.ClearDomainEvents();
                }
            }

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is ChallengeRequest || entry.Entity is ChallengeTrackingHistory)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues != null)
                    {
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        $"Concurrency conflict not supported for {entry.Metadata.Name}");
                }
            }
            throw;
        }
    }
}
