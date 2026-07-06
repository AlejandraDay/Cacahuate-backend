using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class ParentRepository(AppDbContext db) : IParentRepository
{
    public Task<Parent?> GetByUserIdAsync(Guid userId) =>
        db.Parents.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);

    public Task<Parent?> GetByIdAsync(Guid id) =>
        db.Parents.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Parent parent) => await db.Parents.AddAsync(parent);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
