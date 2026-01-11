namespace PartnersHub.Synergy.Application.Interfaces.Common;

/// <summary>
/// Service for sending notifications to users
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send notification when opportunity/SuccessStory/interest is submitted
    /// </summary>
    Task SendSubmittedNotificationAsync(string moduleName, Guid Id, Guid companyId, Guid submitterId,string name, string? companyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when opportunity/SuccessStory/interest is approved by Asset Manager
    /// </summary>
    Task SendApprovedByAssetManagerNotificationAsync(string moduleName, Guid Id, Guid companyId, Guid approverId, string name, string? companyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when opportunity/SuccessStory/interest is published by Synergy Admin
    /// </summary>
    Task SendPublishedNotificationAsync(string moduleName, Guid Id, Guid companyId, Guid publisherId, string name, string? companyName, string? companyEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when opportunity/SuccessStory/interest is rejected
    /// </summary>
    Task SendRejectedNotificationAsync(string moduleName, Guid Id, Guid companyId, string rejectionReason, Guid rejecterId, string name, string? companyName, string? companyEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when opportunity/SuccessStory/interest is updated
    /// </summary>
    Task SendUpdatedNotificationAsync(string moduleName, Guid Id, Guid companyId, string title, string companyName, string companyEmail, CancellationToken cancellationToken = default);
    ///// <summary>
    ///// Send notification when success story is submitted
    ///// </summary>
    //Task SendSuccessStorySubmittedNotificationAsync(Guid successStoryId, Guid companyId, Guid submitterId, CancellationToken cancellationToken = default);

    ///// <summary>
    ///// Send notification when success story is approved by Asset Manager
    ///// </summary>
    //Task SendSuccessStoryApprovedByAssetManagerNotificationAsync(Guid successStoryId, Guid companyId, Guid approverId, CancellationToken cancellationToken = default);

    ///// <summary>
    ///// Send notification when success story is published by Synergy Admin
    ///// </summary>
    //Task SendSuccessStoryPublishedNotificationAsync(Guid successStoryId, Guid companyId, Guid publisherId, CancellationToken cancellationToken = default);

    ///// <summary>
    ///// Send notification when success story is rejected
    ///// </summary>
    //Task SendSuccessStoryRejectedNotificationAsync(Guid successStoryId, Guid companyId, string rejectionReason, Guid rejecterId, CancellationToken cancellationToken = default);
}
