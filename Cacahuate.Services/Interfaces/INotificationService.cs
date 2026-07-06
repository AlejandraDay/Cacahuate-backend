using Cacahuate.Shared.DTOs.Notifications;

namespace Cacahuate.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetMyNotificationsAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
}
