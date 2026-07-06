using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class TherapistRepository(AppDbContext db) : ITherapistRepository
{
    public Task<Therapist?> GetByUserIdAsync(Guid userId) =>
        db.Therapists.Include(t => t.User).FirstOrDefaultAsync(t => t.UserId == userId && t.IsActive);

    public Task<Therapist?> GetByIdAsync(Guid id) =>
        db.Therapists.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

    public Task<List<Therapist>> GetAllActiveAsync() =>
        db.Therapists.Include(t => t.User).Where(t => t.IsActive).ToListAsync();

    public async Task AddAsync(Therapist therapist) => await db.Therapists.AddAsync(therapist);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
