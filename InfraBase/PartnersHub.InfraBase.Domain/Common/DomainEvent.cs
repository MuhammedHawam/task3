using MediatR;

namespace PartnersHub.InfraBase.Domain.Common;

public abstract class DomainEvent : INotification {
    protected DomainEvent() {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.Now;
    }

    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
}