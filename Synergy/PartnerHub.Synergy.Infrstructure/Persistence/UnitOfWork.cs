using MediatR;
using Microsoft.EntityFrameworkCore;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly SynergyDbContext _context;
    private readonly IMediator _mediator;

    public UnitOfWork(SynergyDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all domain events before saving
        var domainEntities = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear domain events to prevent re-processing
        domainEntities.ForEach(e => e.ClearDomainEvents());

        // Save changes to database
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
