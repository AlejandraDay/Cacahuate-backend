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

    public async Task<(List<TherapistRating> Items, int TotalCount, double? AverageStars)> GetByTherapistPagedAsync(Guid therapistId, int page, int pageSize)
    {
        var query = db.TherapistRatings
            .Include(r => r.Parent).ThenInclude(p => p.User)
            .Where(r => r.TherapistId == therapistId);

        var totalCount = await query.CountAsync();
        var averageStars = totalCount > 0 ? await query.AverageAsync(r => (double)r.Stars) : (double?)null;
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount, averageStars);
    }

    public async Task AddAsync(TherapistRating rating) => await db.TherapistRatings.AddAsync(rating);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}