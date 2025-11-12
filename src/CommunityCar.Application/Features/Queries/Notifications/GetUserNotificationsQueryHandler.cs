using CommunityCar.Application.DTOs.Hub.Notifications;
using CommunityCar.Application.Interfaces.Hub;
using MediatR;

namespace CommunityCar.Application.Features.Notifications.Queries;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationService _notificationService;

    public GetUserNotificationsQueryHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await _notificationService.GetUserNotificationsAsync(request.UserId, request.OnlyUnread);
    }
}
