namespace PartnersHub.InfraBase.Domain.Common;

/// <summary>
/// Base class for all aggregate roots with domain events and audit tracking
/// </summary>
public abstract class AggregateRoot : Entity {
    private readonly List<DomainEvent> _domainEvents = new();

    public string? CreatedBy { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent) {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() {
        _domainEvents.Clear();
    }

    protected void MarkAsCreated(string userId) {
        CreatedBy = userId;
        CreatedAt = DateTime.Now;
    }

    protected void MarkAsUpdated(string userId) {
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
    }
}