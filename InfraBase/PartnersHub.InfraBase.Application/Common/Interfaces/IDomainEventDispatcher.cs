using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

/// <summary>
/// Service for dispatching domain events and triggering notifications
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events from an aggregate root and triggers appropriate notifications
    /// </summary>
    Task DispatchEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a collection of domain events
    /// </summary>
    Task DispatchEventsAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken = default);
}
