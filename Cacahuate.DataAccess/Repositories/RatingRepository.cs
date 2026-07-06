using Cacahuate.DataAccess.Context;
using Cacahuate.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cacahuate.DataAccess.Repositories;

public class RatingRepository(AppDbContext db) : IRatingRepository
{
    public Task<TherapistRating?> GetByAppointmentAsync(Guid appointmentId) =>
        db.TherapistRatings.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);

    public Task<List<TherapistRating>> GetByTherapistAsync(Guid therapistId) =>
        db.TherapistRatings
            .Include(r => r.Parent).ThenInclude(p => p.User)
            .Where(r => r.TherapistId == therapistId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(TherapistRating rating) => await db.TherapistRatings.AddAsync(rating);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}