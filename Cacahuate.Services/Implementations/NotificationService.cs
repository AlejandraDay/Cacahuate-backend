using Cacahuate.DataAccess.Repositories;
using Cacahuate.Services.Interfaces;
using Cacahuate.Shared.DTOs.Notifications;

namespace Cacahuate.Services.Implementations;

public class NotificationService(INotificationRepository notificationRepository) : INotificationService
{
    public async Task<List<NotificationResponse>> GetMyNotificationsAsync(Guid userId)
    {
        var notifications = await notificationRepository.GetByUserIdAsync(userId);
        return notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            AppointmentId = n.AppointmentId,
        }).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Not your notification.");

        notification.IsRead = true;
        await notificationRepository.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await notificationRepository.GetByUserIdAsync(userId);
        foreach (var n in notifications.Where(n => !n.IsRead))
            n.IsRead = true;

        await notificationRepository.SaveChangesAsync();
    }
}
