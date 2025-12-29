namespace PartnersHub.Synergy.Application.Interfaces.Common;

/// <summary>
/// Service for audit trail logging
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log an action with details
    /// </summary>
    Task LogActionAsync(
        string entityType, 
        Guid entityId, 
        string action, 
        Guid userId, 
        string? comments = null, 
        CancellationToken cancellationToken = default);
}
