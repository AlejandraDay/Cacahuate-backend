using Cacahuate.DataAccess.Entities;

namespace Cacahuate.DataAccess.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(Guid userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task AddAsync(Notification notification);
    Task AddRangeAsync(IEnumerable<Notification> notifications);
    Task SaveChangesAsync();
}
