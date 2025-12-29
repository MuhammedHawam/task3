namespace PartnersHub.Synergy.Application.Interfaces.Common;

/// <summary>
/// Service for sending notifications to users
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send notification when opportunity is submitted
    /// </summary>
    Task SendOpportunitySubmittedNotificationAsync(Guid opportunityId, Guid companyId, Guid submitterId,string opportunityName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when opportunity is approved by Asset Manager
    /// </summary>
    Task SendOpportunityApprovedByAssetManagerNotificationAsync(Guid opportunityId, Guid companyId, Guid approverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when opportunity is published by Synergy Admin
    /// </summary>
    Task SendOpportunityPublishedNotificationAsync(Guid opportunityId, Guid companyId, Guid publisherId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when opportunity is rejected
    /// </summary>
    Task SendOpportunityRejectedNotificationAsync(Guid opportunityId, Guid companyId, string rejectionReason, Guid rejecterId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when success story is submitted
    /// </summary>
    Task SendSuccessStorySubmittedNotificationAsync(Guid successStoryId, Guid companyId, Guid submitterId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when success story is approved by Asset Manager
    /// </summary>
    Task SendSuccessStoryApprovedByAssetManagerNotificationAsync(Guid successStoryId, Guid companyId, Guid approverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when success story is published by Synergy Admin
    /// </summary>
    Task SendSuccessStoryPublishedNotificationAsync(Guid successStoryId, Guid companyId, Guid publisherId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification when success story is rejected
    /// </summary>
    Task SendSuccessStoryRejectedNotificationAsync(Guid successStoryId, Guid companyId, string rejectionReason, Guid rejecterId, CancellationToken cancellationToken = default);
}
