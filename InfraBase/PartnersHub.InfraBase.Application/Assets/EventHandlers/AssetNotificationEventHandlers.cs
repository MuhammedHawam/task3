using MediatR;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Domain.Events;

namespace PartnersHub.InfraBase.Application.Assets.EventHandlers;

public class AssetSubmittedEventHandler : INotificationHandler<AssetSubmittedEvent>
{
    private readonly INotificationService _notificationService;

    public AssetSubmittedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(AssetSubmittedEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.SubmittedBy,
            title: "New Asset Submitted for Review",
            message: $"Asset {notification.AssetCode} has been submitted and requires your approval.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetSubmission",
            cancellationToken: cancellationToken);
    }
}

public class AssetRejectedByPcAdminEventHandler : INotificationHandler<AssetRejectedByPcAdminEvent>
{
    private readonly INotificationService _notificationService;

    public AssetRejectedByPcAdminEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(AssetRejectedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.RejectedBy,
            title: "Asset Rejected by PC Admin",
            message: $"Asset {notification.AssetCode} was rejected. Reason: {notification.RejectionReason}",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetRejection",
            cancellationToken: cancellationToken);
    }
}

public class AssetAcceptedByPcAdminEventHandler : INotificationHandler<AssetAcceptedByPcAdminEvent>
{
    private readonly INotificationService _notificationService;

    public AssetAcceptedByPcAdminEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(AssetAcceptedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.AcceptedBy,
            title: "Asset Accepted by PC Admin",
            message: $"Asset {notification.AssetCode} has been accepted by PC Admin and requires your review.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetApproval",
            cancellationToken: cancellationToken);
    }
}

public class AssetReturnedForCorrectionEventHandler : INotificationHandler<AssetReturnedForCorrectionByInfrabaseAdminEvent>
{
    private readonly INotificationService _notificationService;

    public AssetReturnedForCorrectionEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(AssetReturnedForCorrectionByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.ReturnedBy,
            title: "Asset Returned for Correction",
            message: $"Asset {notification.AssetCode} needs corrections. Reason: {notification.CorrectionReason}",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetCorrection",
            cancellationToken: cancellationToken);
    }
}

public class AssetCheckedByInfrabaseAdminEventHandler : INotificationHandler<AssetCheckedByInfrabaseAdminEvent>
{
    private readonly INotificationService _notificationService;

    public AssetCheckedByInfrabaseAdminEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(AssetCheckedByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CheckedBy,
            title: "Asset Approved",
            message: $"Asset {notification.AssetCode} has been approved by Infrabase Admin.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetFinalApproval",
            cancellationToken: cancellationToken);
    }
}
