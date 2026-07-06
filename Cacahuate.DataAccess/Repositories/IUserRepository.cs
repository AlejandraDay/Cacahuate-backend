using Cacahuate.DataAccess.Entities;
using Cacahuate.Shared.Enums;

namespace Cacahuate.DataAccess.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetByRoleAsync(UserRole role);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
