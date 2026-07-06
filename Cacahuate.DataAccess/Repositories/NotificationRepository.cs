using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public Task<List<Notification>> GetByUserIdAsync(Guid userId) =>
        db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

    public Task<Notification?> GetByIdAsync(Guid id) =>
        db.Notifications.FirstOrDefaultAsync(n => n.Id == id);

    public async Task AddAsync(Notification notification) =>
        await db.Notifications.AddAsync(notification);

    public async Task AddRangeAsync(IEnumerable<Notification> notifications) =>
        await db.Notifications.AddRangeAsync(notifications);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
