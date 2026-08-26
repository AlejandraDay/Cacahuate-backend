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

    public async Task<(List<Parent> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, string? search)
    {
        var query = db.Parents.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.User.FirstName.ToLower().Contains(term) ||
                p.User.LastName.ToLower().Contains(term) ||
                p.User.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.User.FirstName).ThenBy(p => p.User.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Parent parent) => await db.Parents.AddAsync(parent);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
