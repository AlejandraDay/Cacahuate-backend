using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Cacahuate.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    public Task<User?> GetByIdAsync(Guid id) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

    public Task<List<User>> GetByRoleAsync(UserRole role) =>
        db.Users.Where(u => u.Role == role && u.IsActive).ToListAsync();

    public async Task AddAsync(User user) => await db.Users.AddAsync(user);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
